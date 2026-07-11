namespace O2un.Progression
{
    public interface IExperienceWriter
    {
        void Gain(int amount);
        void Reset();
    }
}
