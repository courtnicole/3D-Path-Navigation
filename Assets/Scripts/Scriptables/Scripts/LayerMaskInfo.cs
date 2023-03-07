namespace PathNav.Interaction
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "LayerMaskInfo", menuName = "Scriptables/Interaction/LayerMaskInfo", order = 5)]
    public class LayerMaskInfo : ScriptableObject
    {
        [SerializeField] private Collider boundingVolumeCollider;

        public Bounds BoundingVolume => boundingVolumeCollider.bounds;

        private static LayerMask _reefSpawnMask;
        private static LayerMask _coralSpawnMask;
        private static LayerMask _terrainSpawnMask;
        private static LayerMask _interactableMask;
        private static LayerMask _boundaryMask;
        private static LayerMask _spawnMask;

        public LayerMask InteractableMask => _interactableMask;
        public LayerMask SpawnMask => _spawnMask;

        public Vector3 BoundingVolumeCenter => BoundingVolume.center;

        public void OnEnable()
        {
            _reefSpawnMask     =  1 << LayerMask.NameToLayer("CanSpawn");
            _interactableMask    =  1 << LayerMask.NameToLayer("Interactable");
            _boundaryMask        =  1 << LayerMask.NameToLayer("SpawnBoundary");
            _spawnMask           =  _reefSpawnMask;
            _spawnMask           &= ~_boundaryMask;
        }
    }
}