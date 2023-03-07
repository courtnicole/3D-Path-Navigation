namespace PathNav.Interaction
{
    using System;
    using UnityEngine;

    [Flags]
    public enum HitTypes
    {
        None = 0,
        Interactable = 1 << 0,
        SpawnPoint = 2 << 0,
        All = Interactable | SpawnPoint,
    }

    public interface ISurfaceHit
    {
        public HitTypes HitType { get; }
        bool HitSurface { get; }

        IInteractable Interactable { get; }
        ISpawnPoint SpawnPoint { get; }

        Vector3 Normal { get; }
        float Distance { get; }
    }
}