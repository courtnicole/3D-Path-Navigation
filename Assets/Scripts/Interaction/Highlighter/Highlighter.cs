//
//  From Outline.cs
//  In the project QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//
//  Modified by Courtney Hutton Pospick 2022
//

namespace PathNav.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Rendering;

    [DisallowMultipleComponent]
    public class Highlighter : MonoBehaviour
    {
        #region Variables
        private static HashSet<Mesh> _registeredMeshes = new();

        public enum Mode
        {
            OutlineAll,
            OutlineVisible,
            OutlineHidden,
            OutlineAndSilhouette,
            SilhouetteOnly,
        }

        public Mode CurrentOutlineMode
        {
            get => _outlineMode;
            set
            {
                _outlineMode = value;
                needsUpdate = true;
            }
        }

        public Color CurrentOutlineColor
        {
            get => outlineColor;
            set
            {
                outlineColor = value;
                needsUpdate  = true;
            }
        }

        public float CurrentOutlineWidth
        {
            get => outlineWidth;
            set
            {
                outlineWidth = value;
                needsUpdate  = true;
            }
        }

        internal bool needsUpdate;

        internal bool initialized;

        [Serializable]
        private class ListVector3
        {
            public List<Vector3> data;
        }

        private Mode _outlineMode = Mode.OutlineVisible;

        [SerializeField] protected Color outlineColor = UnityEngine.Color.white;

        [SerializeField] [Range(0f, 10f)] protected float outlineWidth = 2f;

        [Header("Optional")]
        [SerializeField]
        [Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. " +
                 "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
        private bool precomputeOutline;

        [SerializeField] [HideInInspector] private List<Mesh> bakeKeys = new();

        [SerializeField] [HideInInspector] private List<ListVector3> bakeValues = new();

        private Renderer[] _renderers;
        private Material _outlineMaskMaterial;
        private Material _outlineFillMaterial;

        private static readonly int Color = Shader.PropertyToID("_OutlineColor");
        private static readonly int Width = Shader.PropertyToID("_OutlineWidth");
        private static readonly int ZTestMask = Shader.PropertyToID("_ZTestMask");
        private static readonly int ZTestFill = Shader.PropertyToID("_ZTestFill");

        internal bool OutlineActive { get; set; }
        #endregion

        #region Unity Methods
        internal virtual void OnEnable()
        {
            InitializeLogic();
            InitializePrefabs();
        }

        protected void OnValidate()
        {
            // Update material properties
            needsUpdate = true;

            // Clear cache when baking is disabled or corrupted
            if ((!precomputeOutline && bakeKeys.Count != 0) || bakeKeys.Count != bakeValues.Count)
            {
                bakeKeys.Clear();
                bakeValues.Clear();
            }

            // Generate smooth normals when baking is enabled
            if (precomputeOutline && bakeKeys.Count == 0) Bake();
        }

        protected void OnDestroy()
        {
            DestroyOutlineMaterials();
        }
        #endregion

        #region Initialization
        private void InitializePrefabs()
        {
            // Instantiate outline materials
            _outlineMaskMaterial      = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
            _outlineMaskMaterial.name = "OutlineMask (Instance)";

            _outlineFillMaterial      = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));
            _outlineFillMaterial.name = "OutlineFill (Instance)";
        }

        private void InitializeLogic()
        {
            // Cache renderers
            var initialRenderers = new List<Renderer>(GetComponentsInChildren<Renderer>());

            for (int i = initialRenderers.Count - 1; i >= 0; i--)
            {
                if (initialRenderers[i].GetComponent<DoNotHighlight>())
                    initialRenderers.RemoveAt(i);
            }

            _renderers = initialRenderers.ToArray();

            OutlineActive = false;
        }
        #endregion

        #region Bake Mesh
        private void Bake()
        {
            // Generate smooth normals for each mesh
            var bakedMeshes = new HashSet<Mesh>();

            foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                // Skip duplicates
                if (!bakedMeshes.Add(meshFilter.sharedMesh)) continue;

                // Serialize smooth normals
                List<Vector3> smoothNormals = SmoothNormals(meshFilter.sharedMesh);

                bakeKeys.Add(meshFilter.sharedMesh);
                bakeValues.Add(new ListVector3() { data = smoothNormals, });
            }
        }

        internal void LoadSmoothNormals()
        {
            // Retrieve or generate smooth normals
            foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.GetComponent<DoNotHighlight>()) continue;

                // Skip if smooth normals have already been adopted
                if (!_registeredMeshes.Add(meshFilter.sharedMesh)) continue;

                // Retrieve or generate smooth normals
                int           index         = bakeKeys.IndexOf(meshFilter.sharedMesh);
                List<Vector3> smoothNormals = index >= 0 ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

                // Store smooth normals in UV3
                meshFilter.sharedMesh.SetUVs(3, smoothNormals);

                // Combine submeshes
                Renderer rend = meshFilter.GetComponent<Renderer>();

                if (rend != null) CombineSubmeshes(meshFilter.sharedMesh, rend.sharedMaterials);
            }

            // Clear UV3 on skinned mesh renderers
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMeshRenderer.GetComponent<DoNotHighlight>()) continue;

                // Skip if UV3 has already been reset
                if (!_registeredMeshes.Add(skinnedMeshRenderer.sharedMesh)) continue;

                // Clear UV3
                Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
                sharedMesh.uv4 = new Vector2[sharedMesh.vertexCount];

                // Combine submeshes
                CombineSubmeshes(sharedMesh, skinnedMeshRenderer.sharedMaterials);
            }
        }

        private static List<Vector3> SmoothNormals(Mesh mesh)
        {
            // Group vertices by location
            IEnumerable<IGrouping<Vector3, KeyValuePair<Vector3, int>>> groups =
                mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

            // Copy normals to a new list
            var smoothNormals = new List<Vector3>(mesh.normals);

            // Average normals for grouped vertices
            foreach (IGrouping<Vector3, KeyValuePair<Vector3, int>> group in groups)
            {
                // Skip single vertices
                if (group.Count() == 1) continue;

                // Calculate the average normal
                Vector3 smoothNormal = Vector3.zero;

                foreach (KeyValuePair<Vector3, int> pair in group)
                {
                    smoothNormal += mesh.normals[pair.Value];
                }

                smoothNormal.Normalize();

                // Assign smooth normal to each vertex
                foreach (KeyValuePair<Vector3, int> pair in group)
                {
                    smoothNormals[pair.Value] = smoothNormal;
                }
            }

            return smoothNormals;
        }

        internal void UpdateMaterialProperties()
        {
            // Apply properties according to mode
            _outlineFillMaterial.SetColor(Color, outlineColor);

            switch (_outlineMode)
            {
                case Mode.OutlineAll:
                    _outlineMaskMaterial.SetFloat(ZTestMask, (float)CompareFunction.Always);
                    _outlineFillMaterial.SetFloat(ZTestFill, (float)CompareFunction.Always);
                    _outlineFillMaterial.SetFloat(Width,     outlineWidth);
                    break;

                case Mode.OutlineVisible:
                    _outlineMaskMaterial.SetFloat(ZTestMask, (float)CompareFunction.Always);
                    _outlineFillMaterial.SetFloat(ZTestFill, (float)CompareFunction.LessEqual);
                    _outlineFillMaterial.SetFloat(Width,     outlineWidth);
                    break;

                case Mode.OutlineHidden:
                    _outlineMaskMaterial.SetFloat(ZTestMask, (float)CompareFunction.Always);
                    _outlineFillMaterial.SetFloat(ZTestFill, (float)CompareFunction.Greater);
                    _outlineFillMaterial.SetFloat(Width,     outlineWidth);
                    break;

                case Mode.OutlineAndSilhouette:
                    _outlineMaskMaterial.SetFloat(ZTestMask, (float)CompareFunction.LessEqual);
                    _outlineFillMaterial.SetFloat(ZTestFill, (float)CompareFunction.Always);
                    _outlineFillMaterial.SetFloat(Width,     outlineWidth);
                    break;

                case Mode.SilhouetteOnly:
                    _outlineMaskMaterial.SetFloat(ZTestMask, (float)CompareFunction.LessEqual);
                    _outlineFillMaterial.SetFloat(ZTestFill, (float)CompareFunction.Greater);
                    _outlineFillMaterial.SetFloat(Width,     0f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void CombineSubmeshes(Mesh mesh, IReadOnlyCollection<Material> materials)
        {
            // Skip meshes with a single submesh
            if (mesh.subMeshCount == 1) return;

            // Skip if submesh count exceeds material count
            if (mesh.subMeshCount > materials.Count) return;

            // Append combined submesh
            mesh.subMeshCount++;
            mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
        }
        #endregion

        #region Enable/Disable Outline
        internal void AddOutlineMaterialsToObject()
        {
            foreach (Renderer rend in _renderers)
            {
                // Append outline shaders
                List<Material> materials = rend.sharedMaterials.ToList();

                materials.Add(_outlineMaskMaterial);
                materials.Add(_outlineFillMaterial);

                // Assign materials
                rend.materials = materials.ToArray();
            }
        }

        internal void RemoveOutlineMaterialsFromObject()
        {
            foreach (Renderer rend in _renderers)
            {
                if (rend is null) continue;
                
                // Remove outline shaders
                List<Material> materials = rend.sharedMaterials.ToList();

                materials.Remove(_outlineMaskMaterial);
                materials.Remove(_outlineFillMaterial);

                // Assign materials
                rend.materials = materials.ToArray();
            }
        }

        protected virtual void DestroyOutlineMaterials()
        {
            // Destroy material instance
            Destroy(_outlineMaskMaterial);
            Destroy(_outlineFillMaterial);
        }
        #endregion
    }
}