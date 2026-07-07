namespace O2un.Actors
{
    public interface IActorRegistry
    {
        void Register(IActor actor);
        void Unregister(IActor actor);
    }
}
