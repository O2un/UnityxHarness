using R3;

namespace O2un.ProjectB.Platformer
{
    public interface IRoomDoor
    {
        Observable<string> OnTransitionRequested { get; }
    }
}
