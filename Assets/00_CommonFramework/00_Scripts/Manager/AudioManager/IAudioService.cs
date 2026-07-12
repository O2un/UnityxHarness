using Cysharp.Threading.Tasks;

namespace O2un.Manager
{
    public interface IAudioService
    {
        UniTask PlayBgmAsync(string clipKey);
        void StopBgm();
        void PlaySfx(string clipKey);
        void SetBgmVolume(float volume01);
        void SetSfxVolume(float volume01);
    }
}
