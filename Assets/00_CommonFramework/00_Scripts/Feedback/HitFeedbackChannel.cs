using System;
using R3;

namespace O2un.Feedback
{
    public sealed class HitFeedbackChannel : IHitFeedbackSignal, IHitFeedbackPublisher, IDisposable
    {
        private readonly Subject<HitFeedbackEvent> _onHit = new();

        private bool _disposed;

        public Observable<HitFeedbackEvent> OnHit => _onHit;

        public void Publish(in HitFeedbackEvent hit)
        {
            // 파괴 후 OnNext는 ObjectDisposedException을 던진다. 연출 실패가 전투 경로를 깨지 않도록 삼킨다.
            if (_disposed)
            {
                return;
            }

            _onHit.OnNext(hit);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _onHit.Dispose();
        }
    }
}
