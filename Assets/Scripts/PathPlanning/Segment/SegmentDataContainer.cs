namespace PathNav.PathPlanning
{
    using Extensions;
    using UnityEngine;

    public class SegmentDataContainer : MonoBehaviour
    {
        public IData SegmentData { get; private set; }
        public void Assign(SegmentData data) => SegmentData = data;

        private void OnDestroy()
        {
            UniqueId.Release(SegmentData.Id);
        }
    }
}
