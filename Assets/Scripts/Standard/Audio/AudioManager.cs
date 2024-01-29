namespace PathNav.SceneManagement
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    public class AudioManager : MonoBehaviour
    {
        private static AudioSource _audioSource;
        private AsyncOperationHandle<IList<AudioClip>> _loadHandle;
        private Dictionary<string, AudioClip> _audioClips = new();
        internal static bool IsPlaying => _audioSource.isPlaying;

        private void Awake()
        {
            TryGetComponent(out _audioSource);
            _audioSource.Stop();
        }

        private void OnDestroy()
        {
            _audioClips.Clear();
            if(_loadHandle.IsValid())
                Addressables.Release(_loadHandle);
        }

        public void LoadAudio(List<string> keys)
        {
            LoadAudioClips(keys);
        }
        public void PlayOneShot(Audio.Fx id)
        {
            string address = id.ToString();
            LoadPlayOneShot(address);
        }

        public void PlayClip(string id, float delay = 0)
        {
            LoadPlayAudioClip(id, delay);
        }

        public void PlayClip(Audio.Id id, float delay = 0)
        {
            string address = id.ToString();
            LoadPlayAudioClip(address, delay);
        }
        
        #region Addressable & Audio Methods
        private async void LoadAudioClips(IEnumerable<string> keys)
        {
            _loadHandle = Addressables.LoadAssetsAsync<AudioClip>(keys,
                                                                  addressable => { _audioClips.Add(addressable.name, addressable); },
                                                                  Addressables.MergeMode.UseFirst,
                                                                  false);

            await _loadHandle.Task;
        }

        private static async void LoadPlayOneShot(string address)
        {
            AsyncOperationHandle<AudioClip> loadHandle = Addressables.LoadAssetAsync<AudioClip>(address);

            await loadHandle.Task;

            if (loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(loadHandle);
                return;
            };
            
            _audioSource.PlayOneShot(loadHandle.Result, 1f);

            while (_audioSource.isPlaying)
            {
                await Task.Yield();
            }
            
            _audioSource.clip = null;
            
            Addressables.Release(loadHandle);
        }
        
        private async void LoadPlayAudioClip(string address, float delay = 0)
        {
            bool success = _audioClips.TryGetValue(address, out AudioClip clip);
            if (!success) return;

            while (_audioSource.isPlaying)
            {
                await Task.Yield();
            }

            _audioSource.clip = clip;
            _audioSource.PlayDelayed(delay);

            while (_audioSource.isPlaying)
            {
                await Task.Yield();
            }

            _audioSource.clip = null;
        }
        #endregion
    }
}