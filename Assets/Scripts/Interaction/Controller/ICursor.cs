namespace PathNav.Interaction
{
    using UnityEngine;

    public interface ICursor : ICollidable
    {
        public Transform Cursor { get; }

        public Vector3 Position => Cursor.position;

        void SetCursorObject(GameObject obj);
    }
}
