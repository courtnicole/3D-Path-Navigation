using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    using Dreamteck.Splines;
    using System;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.XR;
    public class XRControllerBasic : MonoBehaviour
    {
        [SerializeField] private TrackedPoseDriver controllerPose;
        
        public InputActionReference triggerClick;

        [SerializeField] private Transform drawingPlane;
        [SerializeField] private SplineComputer spline;

        private SplinePoint[] CurrentPoints
        {
            get => spline.GetPoints();
            set => spline.SetPoints(value);
        }
        private Vector3 PlaneOrigin  => drawingPlane.position;
        private Vector3 PlaneNormal  => drawingPlane.up;
        private Vector3 DrawPosition => controllerPose.transform.position;
        private Vector3 DrawRotation => controllerPose.transform.position;
        private Vector3 _currentPosition;
        
        private Matrix4x4 LocalToWorld => drawingPlane.localToWorldMatrix;
        private Matrix4x4 WorldToLocal => drawingPlane.worldToLocalMatrix;
        private SplinePoint[] _oldPoints;
        private SplinePoint[] _newPoints;

        private bool _drawingEnabled;

        private int CurrentPointCount => spline.pointCount;
        
        private void OnEnable()
        {
            EnableActions();
            _oldPoints = new SplinePoint[2];
            _newPoints = new SplinePoint[2];
        }
        
        private void OnDisable()
        {
            DisableActions();
        }

        private void EnableActions()
        {
            triggerClick.action.Enable();
            triggerClick.action.performed += TriggerDown;
            triggerClick.action.canceled  += TriggerUp;
        }

        private void DisableActions()
        {
            triggerClick.action.performed -= TriggerDown;
        }

        private void Update()
        {

            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                _currentPosition = Vector3.ProjectOnPlane(DrawPosition, PlaneNormal);
                Debug.Log($"{DrawPosition}, {PlaneOrigin}, {PlaneNormal}, {_currentPosition}");
                
            }
            if(!_drawingEnabled) return;

            _currentPosition = Vector3.ProjectOnPlane(DrawPosition - PlaneOrigin, PlaneNormal);
            AddPoint(_currentPosition);
        }

        private void TriggerUp(InputAction.CallbackContext obj)
        {
            _drawingEnabled = false;
        }

        private void TriggerDown(InputAction.CallbackContext obj)
        {
            StartDraw();
        }

        private void StartDraw()
        {
            Vector3 shifted = PlaneOrigin + (-3.75f * drawingPlane.forward);
            AddFirstPoint(shifted, -drawingPlane.forward);
            _drawingEnabled = true;
        }

        private void Draw()
        {
            _currentPosition = Vector3.ProjectOnPlane(DrawPosition - PlaneOrigin, PlaneNormal);
            _currentPosition = WorldToLocal.MultiplyPoint3x4(_currentPosition);
            AddPoint(_currentPosition);
        }
        
        public void AddFirstPoint(Vector3 newPosition, Vector3 heading)
        {
            Vector3 end = newPosition + (heading.normalized * 0.01f);

            SplinePoint pt1 = new()
            {
                type     = SplinePoint.Type.SmoothMirrored,
                color    = Color.white,
                normal   = Vector3.up,
                size     = 0.01f,
                tangent  = Vector3.forward,
                position = newPosition,
            };

            SplinePoint pt2 = new()
            {
                type     = SplinePoint.Type.SmoothMirrored,
                color    = Color.white,
                normal   = Vector3.up,
                size     = 0.01f,
                tangent  = Vector3.forward,
                position = end,
            };

            _newPoints    = new SplinePoint[2];
            _newPoints[0] = pt1;
            _newPoints[1] = pt2;

            CurrentPoints = _newPoints;
            
            _newPoints = null;
        }
        
        public void AddPoint(Vector3 position)
        {
            _oldPoints = CurrentPoints;
            Array.Resize(ref _newPoints, CurrentPointCount + 1);

            for (int i = 0; i < CurrentPointCount; i++)
            {
                _newPoints[i] = _oldPoints[i];
            }

            _newPoints[^1]      = new SplinePoint(position)
            {
                size = 0.01f,
            };
            CurrentPoints       = _newPoints;
            
            
            Array.Clear(_oldPoints, 0, _oldPoints.Length);
            Array.Clear(_newPoints, 0, _newPoints.Length);
        }
    }
}
