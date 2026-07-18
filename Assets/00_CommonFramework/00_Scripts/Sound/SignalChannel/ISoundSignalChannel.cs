using R3;

namespace O2un.Sound
{
    public interface ISoundSignalPublisher
    {
        void Publish(in SoundSignal signal);
    }

    public interface ISoundSignalSource
    {
        Observable<SoundSignal> OnSound { get; }
    }
}
