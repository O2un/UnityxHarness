namespace O2un.ProjectB.Platformer
{
    public interface IAttackHook
    {
        bool IsFinished { get; }

        void Begin();
        void Tick(float dt);
    }
}
