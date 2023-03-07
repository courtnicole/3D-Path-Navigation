namespace PathNav.Patterns.Factory
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using UnityEngine.ResourceManagement.ResourceProviders;

    public class Factory
    {
        private readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new();

        public async Task<T> LoadFromKeyAsync<T>(AssetReference key)
        {
            if (!KeyIsValid(key)) return default;

            Task<IList<IResourceLocation>> resourceLocation = GetResourceLocation<T>(key);

            await resourceLocation;

            if (!resourceLocation.IsCompletedSuccessfully) Debug.LogError("Failure");

            List<AsyncOperationHandle> loadOperations = new(resourceLocation.Result.Count);

            T result = default;

            foreach (IResourceLocation location in resourceLocation.Result)
            {
                AsyncOperationHandle<T> loadAsset = Addressables.LoadAssetAsync<T>(location);

                await loadAsset.Task;

                if (loadAsset.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedAssets.Add(location.PrimaryKey, loadAsset);
                    result = loadAsset.Result;
                    loadOperations.Add(loadAsset);
                }
                else
                    Addressables.Release(loadAsset);
            }

            AsyncOperationHandle groupOperation = Addressables.ResourceManager.CreateGenericGroupOperation(loadOperations, true);

            await groupOperation.Task;

            return result;
        }

        public async Task<T> LoadFromStringAsync<T>(string key)
        {
            Task<IList<IResourceLocation>> resourceLocation = GetResourceLocation<T>(key);

            await resourceLocation;

            if (!resourceLocation.IsCompletedSuccessfully) Debug.LogError("Failure");

            List<AsyncOperationHandle> loadOperations = new(resourceLocation.Result.Count);

            T result = default;

            foreach (IResourceLocation location in resourceLocation.Result)
            {
                AsyncOperationHandle<T> loadAsset = Addressables.LoadAssetAsync<T>(location);

                await loadAsset.Task;

                if (loadAsset.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedAssets.Add(location.PrimaryKey, loadAsset);
                    result = loadAsset.Result;
                    loadOperations.Add(loadAsset);
                }
                else
                    Addressables.Release(loadAsset);
            }

            AsyncOperationHandle groupOperation = Addressables.ResourceManager.CreateGenericGroupOperation(loadOperations, true);

            await groupOperation.Task;

            return result;
        }

        public async Task<GameObject> InstantiateObjectAsync(AssetReference key, InstantiationParameters parameters, CancellationToken cancellationToken)
        {
            if (!KeyIsValid(key)) return null;

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, parameters);

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject obj = handle.Result;
                handle.Result.AddComponent(typeof(ReleaseAddressable));
                return obj;
            }

            Addressables.Release(handle);
            return null;
        }

        public void ReleaseAllLoadedAssets()
        {
            foreach (KeyValuePair<string, AsyncOperationHandle> item in _loadedAssets)
            {
                Addressables.Release(item.Value);
            }
        }

        public void ReleaseLoadedAsset<T>(AssetReference key)
        {
            Task<IList<IResourceLocation>> locations = GetResourceLocation<T>(key);

            foreach (IResourceLocation location in locations.Result)
            {
                if (_loadedAssets.TryGetValue(location.PrimaryKey, out AsyncOperationHandle handle)) Addressables.Release(handle);
            }
        }

        #region Helper Functions
        private static bool KeyIsValid(IKeyEvaluator key) => key.RuntimeKeyIsValid();

        private static async Task<IList<IResourceLocation>> GetResourceLocation<T>(AssetReference key)
        {
            IList<AssetReference> keys = new List<AssetReference> { key, };

            AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(T));

            await locations.Task;

            return locations.Result;
        }

        private static async Task<IList<IResourceLocation>> GetResourceLocation<T>(string key)
        {
            IList<string> keys = new List<string> { key, };

            AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(T));

            await locations.Task;

            return locations.Result;
        }
        #endregion
    }
}