using Cysharp.Threading.Tasks;
using R3;

namespace O2un.ProjectB.Platformer
{
    public enum RoomState
    {
        Idle,
        Loading,
        Playing,
        Cleared,
        Transitioning,
        Finished,
    }

    public interface IRoomProgression
    {
        Observable<int> OnRoomEntered { get; }
        Observable<Unit> OnRoomCleared { get; }
        Observable<Unit> OnStageCleared { get; }
        Observable<Unit> OnLoadFailed { get; }
        UniTask BeginStageAsync();
        void RequestTransition(string destinationId);
    }
}
