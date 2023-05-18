namespace PathNav.ExperimentControl
{
    using UnityEngine.AddressableAssets;
    using UnityEngine;

    public enum Type
    {
        SEQ,
        Discomfort,
        Handedness,
    }
    
    [CreateAssetMenu(fileName = "Survey", menuName = "Scriptables/Standard/Survey", order = 300)]
    public class Survey : ScriptableObject
    {
        [Header("Survey Information")]
        
        [SerializeField] private Type type;
        public Type Type => type;

        public AssetReferenceGameObject prefab;
        
        [Header("Scene Information")]
        public string sceneName;
    }
}
