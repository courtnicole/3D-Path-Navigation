namespace PathNav.ExperimentControl
{
    using UnityEngine;
    [CreateAssetMenu(fileName = "ModelOrder", menuName = "Scriptables/Standard/ModelOrder", order = 250)]
    public class ModelOrder : ScriptableObject
    {
        [SerializeField] private int[] modelIndex;

        public int[] ModelIndex
        {
            get => modelIndex;
            set => modelIndex = value;
        }
    }
}
