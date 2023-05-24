
namespace PathNav.ExperimentControl
{
    using Extensions;
    using PathPlanning;
    using UnityEngine;

    public class ModelDataContainer : MonoBehaviour
    {
        public IData ModelData              { get; private set; }
        public void  Assign(ModelData data) => ModelData = data;

        private void OnDestroy()
        {
            UniqueId.Release(ModelData.Id);
        }
    }
}
