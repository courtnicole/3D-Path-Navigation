namespace PathNav.Interaction
{
    using Events;
    using UnityEngine;

    public class CursorElement : MonoBehaviour, ICursor
    {
        [SerializeField] private Collider colliderElement;
        [SerializeField] private Transform cursor;

        #region Implementation of ICollidable
        public Collider ColliderElement => colliderElement;
        private ICollidable Collidable => this;

        private void OnTriggerEnter(Collider objectHit)
        {
            Collidable.TriggerEntered(EventId.WandCursorTriggerEntered, objectHit);
        }
        #endregion

        #region Implementation of ICursor
        public Transform Cursor => cursor;
        public void SetCursorObject(GameObject obj) { }
        #endregion
    }
}