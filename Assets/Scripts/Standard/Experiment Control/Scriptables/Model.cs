namespace PathNav.ExperimentControl
{
    using UnityEngine.AddressableAssets;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Model", menuName = "Scriptables/Standard/Model", order = 400)]
    public class Model : ScriptableObject
    {
       [Header("Model Information")]
       public AssetReferenceGameObject assetReference;
       
       [SerializeField] private Vector3 translation;
       public Vector3 Translation
       {
           get => translation;
           set => translation = value;
       }

       [SerializeField] private Vector3 rotation;
       public Vector3 Rotation
       {
           get => rotation;
           set => rotation = value;
       }

       [SerializeField] private float scale;
       public float Scale
       {
           get => scale;
           set => scale = value;
       }
    }
}
