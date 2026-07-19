namespace O2un.Feedback
{
    public interface IHitFeedbackPublisher
    {
        void Publish(in HitFeedbackEvent hit);
    }
}
