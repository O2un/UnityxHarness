using R3;

namespace O2un.Feedback
{
    public interface IHitFeedbackSignal
    {
        Observable<HitFeedbackEvent> OnHit { get; }
    }
}
