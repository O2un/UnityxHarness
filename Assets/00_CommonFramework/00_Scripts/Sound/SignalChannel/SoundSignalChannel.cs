using System;
using R3;

namespace O2un.Sound
{
    public sealed class SoundSignalChannel : ISoundSignalPublisher, ISoundSignalSource, IDisposable
    {
        private readonly Subject<SoundSignal> _onSound = new();

        public Observable<SoundSignal> OnSound => _onSound;

        public void Publish(in SoundSignal signal)
        {
            _onSound.OnNext(signal);
        }

        public void Dispose()
        {
            _onSound.Dispose();
        }
    }
}
