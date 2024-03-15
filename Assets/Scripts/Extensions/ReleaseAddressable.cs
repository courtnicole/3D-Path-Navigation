namespace PathNav.Extensions
{
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class ReleaseAddressable : MonoBehaviour
    {
         private void OnDestroy()
        {
            Addressables.ReleaseInstance(gameObject);
        }
    }
}
