namespace PathNav.PathPlanning
{
    using Interaction;
    using UnityEngine;

    public class SegmentStartPoint : MonoBehaviour, IPlaceable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject childObject;

        public void Hide() => childObject.SetActive(false);
        public void Show() => childObject.SetActive(true);

        private Transform RootTransform => transform;

        private static readonly int IsBeingPlaced = Animator.StringToHash("IsBeingPlaced");

        public void Attach(Transform t)
        {
            RootTransform.parent = t;
            RootTransform.localRotation = Quaternion.identity;
            RootTransform.localPosition = Vector3.zero;
        }

        public void Detach()
        {
            if (animator != null)
            {
                animator.SetBool(IsBeingPlaced, false);
            }

            RootTransform.parent = null;
            RootTransform.rotation = Quaternion.Euler(0, RootTransform.rotation.y, 0);
        }
    }
}
