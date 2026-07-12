using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace O2un.Manager
{
    public sealed class AudioManager : IAudioService
    {
        private readonly IAssetService _assetService;
        private readonly AudioPlayerView _view;
        private readonly Dictionary<string, AudioClip> _sfxCache = new();
        private readonly HashSet<string> _sfxLoading = new();

        private string _currentBgmKey;
        private int _bgmRequestToken;

        public AudioManager(IAssetService assetService, AudioPlayerView view)
        {
            _assetService = assetService;
            _view = view;
        }

        public async UniTask PlayBgmAsync(string clipKey)
        {
            if (string.IsNullOrEmpty(clipKey))
            {
                return;
            }

            int token = ++_bgmRequestToken;
            _view.StopBgm();

            AudioClip clip = await _assetService.LoadAsync<AudioClip>(clipKey);

            if (token != _bgmRequestToken)
            {
                return;
            }

            _currentBgmKey = clipKey;
            _view.PlayBgm(clip);
        }

        public void StopBgm()
        {
            _bgmRequestToken++;
            _currentBgmKey = null;
            _view.StopBgm();
        }

        public void PlaySfx(string clipKey)
        {
            if (string.IsNullOrEmpty(clipKey))
            {
                return;
            }

            if (_sfxCache.TryGetValue(clipKey, out AudioClip cached))
            {
                _view.PlaySfxOneShot(cached);
                return;
            }

            if (true == _sfxLoading.Contains(clipKey))
            {
                return;
            }

            LoadAndPlaySfxAsync(clipKey).Forget();
        }

        public void SetBgmVolume(float volume01)
        {
            _view.SetBgmVolume(Mathf.Clamp01(volume01));
        }

        public void SetSfxVolume(float volume01)
        {
            _view.SetSfxVolume(Mathf.Clamp01(volume01));
        }

        private async UniTaskVoid LoadAndPlaySfxAsync(string clipKey)
        {
            _sfxLoading.Add(clipKey);

            try
            {
                AudioClip clip = await _assetService.LoadAsync<AudioClip>(clipKey);
                _sfxCache[clipKey] = clip;
                _view.PlaySfxOneShot(clip);
            }
            finally
            {
                _sfxLoading.Remove(clipKey);
            }
        }
    }
}
