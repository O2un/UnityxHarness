using UnityEngine;

namespace O2un.Manager
{
    public sealed class AudioPlayerView : MonoBehaviour
    {
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;

        private void Reset()
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
        }

        public void PlayBgm(AudioClip clip)
        {
            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        public void StopBgm()
        {
            _bgmSource.Stop();
            _bgmSource.clip = null;
        }

        public void PlaySfxOneShot(AudioClip clip)
        {
            _sfxSource.PlayOneShot(clip);
        }

        public void SetBgmVolume(float volume01)
        {
            _bgmSource.volume = volume01;
        }

        public void SetSfxVolume(float volume01)
        {
            _sfxSource.volume = volume01;
        }
    }
}
