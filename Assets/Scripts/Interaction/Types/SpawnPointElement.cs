namespace PathNav.Interaction
{
    using Input;
    using UnityEngine;

    public class SpawnPointElement : ISpawnPoint
    {
        public ISpawnPoint SpawnPoint => this;
        public Surface Space { get; set; }
        public Vector3 Position { get; set; }
        public Transform Plane { get; set; }

        public SpawnPointElement()
        {
            Space    = Surface.None;
            Position = Vector3.zero;
            Plane = null;
        }

        public SpawnPointElement(Vector3 position, Surface space = Surface.Ground)
        {
            Space    = space;
            Position = position;
        }

        public SpawnPointElement(Vector3 position, Transform plane, Surface space = Surface.Ground)
        {
            Space    = space;
            Position = position;
            Plane = plane;
        }

        public void Set(Vector3 position, Surface space = Surface.Ground)
        {
            Space    = space;
            Position = position;
        }
    }
}