namespace O2un.AI
{
    public interface IState
    {
        void Enter();
        void Tick(float dt);
        void Exit();
    }
}
