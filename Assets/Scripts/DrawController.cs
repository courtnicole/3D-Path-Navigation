namespace PathNav
{
    using System;
    using Unity.Mathematics;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.XR;
    using UnityEngine;

    public class DrawController : MonoBehaviour
    {
        [SerializeField] private ComputeShader drawComputeShader;
        [SerializeField] private TrackedPoseDriver controllerPose;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform poseSource;
        [SerializeField] private Transform angleParent;
        [SerializeField] private Transform transportTarget;
        [SerializeField] private Mesh transportMesh;

        public InputActionReference triggerClick;
        public InputActionReference angleControl;
        public InputActionReference travelClick;
        public InputActionReference eraseClick;

        private RenderTexture _canvasRenderTexture;

        private bool _drawingEnabled;

        private int _drawTextureKernel;
        private int _initBackgroundKernel;
        private int _readPixelsKernel;
        private Vector3 _previousPosition;
        private Vector3 _position;
        private Vector4 _plane;

        private Material _material;
        private static readonly int MainTex = Shader.PropertyToID("_BaseMap");

        private Vector3 ControllerPosition => controllerPose.transform.position;
        private Vector3 ControllerRotation => controllerPose.transform.forward;

        private uint _xGroupSize;
        private uint _yGroupSize;

        private int _width;
        private int _height;

        private RaycastHit _hit;
        private bool _isHit;

        private ComputeBuffer _pixelAppendBuffer;
        private ComputeBuffer _countBuffer;
        private float3[] _pixelPositions;
        private GameObject[] _display;
        private const int _stepSize = 200;

        private int[] _triangleIndex;
        private Vector3[] _vertexMap;
        private Ray _ray;
        private Matrix4x4 _trs;
        private int _lastIndex;

        private void OnEnable()
        {
            _drawingEnabled = false;

            _width  = 512;
            _height = 512;

            _canvasRenderTexture                   = new RenderTexture(_width, _height, 16);
            _canvasRenderTexture.filterMode        = FilterMode.Point;
            _canvasRenderTexture.enableRandomWrite = true;
            _canvasRenderTexture.Create();

            _initBackgroundKernel = drawComputeShader.FindKernel("init");
            _drawTextureKernel    = drawComputeShader.FindKernel("draw_texture");
            _readPixelsKernel     = drawComputeShader.FindKernel("read_pixels");

            drawComputeShader.SetInt("_CanvasWidth",  _width);
            drawComputeShader.SetInt("_CanvasHeight", _height);
            drawComputeShader.SetBool("clear_canvas", true);
            drawComputeShader.SetTexture(_initBackgroundKernel, "canvas", _canvasRenderTexture);
            drawComputeShader.GetKernelThreadGroupSizes(_initBackgroundKernel, out uint xGroupSize, out uint yGroupSize, out _);

            drawComputeShader.Dispatch(_initBackgroundKernel,
                                       Mathf.CeilToInt(_canvasRenderTexture.width  / (float)xGroupSize),
                                       Mathf.CeilToInt(_canvasRenderTexture.height / (float)yGroupSize),
                                       1);

            drawComputeShader.GetKernelThreadGroupSizes(_drawTextureKernel, out _xGroupSize, out _yGroupSize, out _);

            _material = GetComponent<MeshRenderer>().material;
            _material.SetTexture(MainTex, _canvasRenderTexture);

            CreateMeshData();

            _pixelPositions = new float3[_width * _height];
            _ray            = new Ray();
            CreateBuffers();
            CreatePathDisplay();

            EnableActions();
        }

        private void OnDisable()
        {
            _pixelAppendBuffer?.Release();
            _countBuffer?.Release();

            _pixelAppendBuffer = null;
            _countBuffer       = null;

            DisableActions();
        }

        private void CreateBuffers()
        {
            _pixelAppendBuffer ??= new ComputeBuffer(_width * _height, sizeof(float) * 3, ComputeBufferType.Append);
            _pixelAppendBuffer.SetCounterValue(0);

            _countBuffer ??= new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        }

        private void CreateMeshData()
        {
            _triangleIndex = transportMesh.triangles;
            _vertexMap     = transportMesh.vertices;

            Vector3 t = new(0, -160, 0);
            Vector3 r = new(90, 0, 0);
            Vector3 s = new(1, 1, 1);
            _trs = Matrix4x4.TRS(t, Quaternion.Euler(r), s);
        }

        private void CreatePathDisplay()
        {
            _display = new GameObject[_stepSize];

            for (int i = 0; i < _stepSize; i++)
            {
                _display[i]      = Instantiate(prefab, Vector3.zero, quaternion.identity);
                _display[i].name = $"Pixel {i}";
                _display[i].SetActive(false);
            }
        }

        private void EnableActions()
        {
            triggerClick.action.Enable();
            triggerClick.action.started  += TriggerDown;
            triggerClick.action.canceled += TriggerUp;

            angleControl.action.Enable();
            angleControl.action.performed += AngleChange;

            eraseClick.action.Enable();
            eraseClick.action.performed += Erase;

            travelClick.action.Enable();
            travelClick.action.performed += Travel;
        }

        private void DisableActions()
        {
            triggerClick.action.started   -= TriggerDown;
            triggerClick.action.canceled  -= TriggerUp;
            angleControl.action.performed -= AngleChange;
            eraseClick.action.performed   -= AngleChange;
            travelClick.action.performed  -= Travel;
        }

        private void Update()
        {
            _ray.origin    = ControllerPosition;
            _ray.direction = ControllerRotation;

            _isHit = Physics.Raycast(_ray, out _hit, 3.0f);

            if (!_isHit)
            {
                return;
            }

            if (!_drawingEnabled)
            {
                _previousPosition = new Vector3(512.0f * _hit.textureCoord.x, 512.0f * _hit.textureCoord.y, 0.0f);
                return;
            }

            _lastIndex = _hit.triangleIndex * 3;

            _position = new Vector3(512.0f * _hit.textureCoord.x, 512.0f * _hit.textureCoord.y, 0.0f);

            drawComputeShader.SetFloats("previous_position", _previousPosition.x, _previousPosition.y, _previousPosition.z, 0.0f);
            drawComputeShader.SetFloats("position",          _position.x,         _position.y,         _position.z,         0.0f);

            drawComputeShader.SetTexture(_drawTextureKernel, "canvas", _canvasRenderTexture);
            drawComputeShader.SetBuffer(_drawTextureKernel, "pixels", _pixelAppendBuffer);

            drawComputeShader.Dispatch(_drawTextureKernel,
                                       Mathf.CeilToInt(_canvasRenderTexture.width / (float)_xGroupSize),
                                       Mathf.CeilToInt(_canvasRenderTexture.width / (float)_yGroupSize),
                                       1);
            _previousPosition = _position;
        }

        private void GetPosition()
        {
            Vector3 p0 = _vertexMap[_triangleIndex[_lastIndex + 0]];
            Vector3 p1 = _vertexMap[_triangleIndex[_lastIndex + 1]];
            Vector3 p2 = _vertexMap[_triangleIndex[_lastIndex + 2]];
            Vector3 target = (p0 + p1 + p2) / 3.0f;
            target = _trs.MultiplyPoint3x4(target);
            transportTarget.position = target;
        }

        private void Erase(InputAction.CallbackContext obj)
        {
            Reset();
        }

        private void Reset()
        {
            drawComputeShader.SetInt("_CanvasWidth",  _width);
            drawComputeShader.SetInt("_CanvasHeight", _height);
            drawComputeShader.SetBool("clear_canvas", true);
            drawComputeShader.SetTexture(_initBackgroundKernel, "canvas", _canvasRenderTexture);
            drawComputeShader.GetKernelThreadGroupSizes(_initBackgroundKernel, out uint xGroupSize, out uint yGroupSize, out _);

            drawComputeShader.Dispatch(_initBackgroundKernel,
                                       Mathf.CeilToInt(_canvasRenderTexture.width  / (float)xGroupSize),
                                       Mathf.CeilToInt(_canvasRenderTexture.height / (float)yGroupSize),
                                       1);
        }

        private void Travel(InputAction.CallbackContext obj)
        {
            GetPosition();
        }

        private void AngleChange(InputAction.CallbackContext obj)
        {
            Vector2 change = obj.ReadValue<Vector2>();
            float   sign   = -Mathf.Sign(change.y);
            if (sign == 0) return;

            Vector3 currentEulerAngles = angleParent.localEulerAngles;
            currentEulerAngles.x         += 1.0f * sign;
            currentEulerAngles.x         =  Mathf.Clamp(currentEulerAngles.x, 0f, 75.0f);
            angleParent.localEulerAngles =  currentEulerAngles;
        }

        private void TriggerUp(InputAction.CallbackContext obj)
        {
            _drawingEnabled = false;
            int[] count = { 0, };
            ComputeBuffer.CopyCount(_pixelAppendBuffer, _countBuffer, 0);
            _countBuffer.GetData(count);

            int arraySize = count[0];
            arraySize = Mathf.Min(arraySize, _height * _width);

            Array.Resize(ref _pixelPositions, arraySize);
            _pixelAppendBuffer.GetData(_pixelPositions);

            Display();
        }

        private float CurrentAngle() => angleParent.localEulerAngles.x;

        private void TriggerDown(InputAction.CallbackContext obj)
        {
            Reset();

            _drawingEnabled = true;
            _pixelAppendBuffer.SetCounterValue(0);
            drawComputeShader.SetFloat("size",      8.0f);
            drawComputeShader.SetFloat("smoothing", 0.1f);
            Physics.Raycast(ControllerPosition, ControllerRotation, out _hit);
            _previousPosition = new Vector3(512 * _hit.textureCoord.x, 512 * _hit.textureCoord.y, 0.0f);
        }

        private void Display()
        {
            if (_pixelPositions.Length == 0) return;

            int     step     = Mathf.CeilToInt(_pixelPositions.Length / (float)_stepSize);
            Vector3 position = poseSource.position;
            position.y = 0.0f;

            float x0     = position.x - (_pixelPositions[0].x * 10);
            float yShift = position.y - Height(Remap(_pixelPositions[0].z * 10), 0);
            float z0     = position.z - (_pixelPositions[0].z * 10);

            for (int i = 0; i < _pixelPositions.Length; i += step)
            {
                position.x = x0 + (_pixelPositions[i].x * 10);
                position.y = Height(Remap(_pixelPositions[i].z * 10), yShift);
                position.z = z0 + (_pixelPositions[i].z * 10);

                _display[i / step].transform.position = position;
                _display[i / step].SetActive(true);
            }

            // Vector2 uv = new(_previousPosition.x * 0.001953125f, _previousPosition.y * 0.001953125f);
            //
            // if (!_uvToPoseMap.TryGetValue(uv, out Vector3 pose)) return;
            //
            // pose                     = _trs.MultiplyPoint3x4(pose);
            // transportTarget.position = pose;
        }

        private static float Remap(float i) => 0.5f * (i + 1.0f);

        private float Height(float i, float shift) => i * Mathf.Sin(CurrentAngle() * Mathf.Deg2Rad) + shift;
    }
}