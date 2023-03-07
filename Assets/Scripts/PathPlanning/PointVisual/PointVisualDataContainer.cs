namespace PathNav.PathPlanning
{
    using Extensions;
    using UnityEngine;

    public class PointVisualDataContainer : MonoBehaviour
    {
        public IData PointVisualData { get; private set; }
        public void Assign(PointVisualData data) => PointVisualData = data;

        private void OnDestroy()
        {
            UniqueId.Release(PointVisualData.Id);
        }
    }
}