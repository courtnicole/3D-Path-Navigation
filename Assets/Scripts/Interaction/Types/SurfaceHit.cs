namespace PathNav.Interaction
{
    using UnityEngine;

    public class SurfaceHit : ISurfaceHit
    {
        public ISurfaceHit Hit => this;

        public HitTypes HitType
        {
            get
            {
                if (!HitSurface) return HitTypes.None;
                if (Interactable != null) return HitTypes.Interactable;
                if (SpawnPoint   != null) return HitTypes.SpawnPoint;

                return HitTypes.None;
            }
        }

        public bool HitSurface => Interactable != null || SpawnPoint != null;
        
        public IInteractable Interactable { get; set; }
        public ISpawnPoint SpawnPoint { get; set; }

        public Vector3 Normal { get; set; }
        public float Distance { get; set; }

        public SurfaceHit(RaycastHit hit, ISpawnPoint spawnPoint)
        {
            Interactable = null;
            SpawnPoint   = spawnPoint;
            Normal       = hit.normal;
            Distance     = hit.distance;
        }

        public SurfaceHit(RaycastHit hit, IInteractable interactable)
        {
            Interactable = interactable;
            SpawnPoint   = null;
            Normal       = hit.normal;
            Distance     = hit.distance;
        }

        public SurfaceHit(float distance)
        {
            Interactable = null;
            SpawnPoint   = null;
            Normal       = Vector3.up;
            Distance     = distance;
        }

        public SurfaceHit()
        {
            Interactable = null;
            SpawnPoint   = null;
            Normal       = Vector3.zero;
            Distance     = 99999f;
        }
    }
}