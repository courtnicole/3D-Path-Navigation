namespace PathNav.Interaction
{
    using System;
    using Input;
    using UnityEngine;

    public enum Surface
    {
        None,
        Ground,
        MiniatureModel,
    }

    public interface ISpawnPoint
    {
        Surface Space { get; }
        Vector3 Position { get; }

        Transform Plane { get; }
    }

    public class SpawnPointEventArgs : EventArgs
    {
        public SpawnPointEventArgs(ISpawnPoint spawnPoint, IController controller)
        {
            SpawnPoint = spawnPoint;
            Controller = controller;
        }

        public ISpawnPoint SpawnPoint { get; }
        public IController Controller { get; }
    }
}
