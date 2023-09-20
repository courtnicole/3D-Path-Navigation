namespace PathNav.SceneManagement
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    public class AudioManager : MonoBehaviour
    {
        private static AudioSource _audioSource;
        private static AudioManager _instance;

        private AsyncOperationHandle<IList<AudioClip>> _loadHandle;
        private List<string> _keys = new() { "audio", "fx", };
        private Dictionary<string, AudioClip> _audioClips = new();

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.Stop();
            _instance = this;
            StartCoroutine(LoadAudioClips());
        }

        private void OnDestroy()
        {
            Addressables.Release(_loadHandle);
        }

        public void PlayOneShot(Audio.Fx id)
        {
            string address = id.ToString();
            _instance.StartCoroutine(LoadPlayOneShot(address));
        }

        public void PlayClip(Audio.Id id, float delay = 0)
        {
            string address = id.ToString();
            _instance.StartCoroutine(LoadPlayAudioClip(address, delay));
        }

        private IEnumerator LoadPlayAudioClip(string address, float delay = 0)
        {
            bool success = _audioClips.TryGetValue(address, out AudioClip clip);
            if (!success) yield break;

            yield return new WaitUntil(() => !_audioSource.isPlaying);

            _audioSource.clip = clip;
            _audioSource.PlayDelayed(delay);

            yield return new WaitUntil(() => !_audioSource.isPlaying);

            _audioSource.clip = null;
        }

        #region Addressable Methods
        private IEnumerator LoadAudioClips()
        {
            _loadHandle = Addressables.LoadAssetsAsync<AudioClip>(_keys,
                                                                  addressable => { _audioClips.Add(addressable.name, addressable); },
                                                                  Addressables.MergeMode.Union,
                                                                  false);

            yield return _loadHandle;
        }

        private static IEnumerator LoadPlayOneShot(string address)
        {
            AsyncOperationHandle<AudioClip> clip = Addressables.LoadAssetAsync<AudioClip>(address);

            if (!clip.IsDone)
                yield return clip;

            if (clip.Status != AsyncOperationStatus.Succeeded) yield break;

            _audioSource.PlayOneShot(clip.Result, 1f);

            yield return new WaitUntil(() => !_audioSource.isPlaying);

            _audioSource.clip = null;
            Addressables.Release(clip);
        }
        #endregion
    }
}