using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public interface IRoomSignalPublisher
    {
        void RequestSpawnBegin();
        void PublishRoomCleared();
        void PublishRoomReady(Vector3 playerSpawnPosition);
    }

    public interface IRoomSignalSource
    {
        Observable<Unit> OnSpawnBeginRequested { get; }
        Observable<Unit> OnRoomCleared { get; }
        Observable<Vector3> OnRoomReady { get; }
    }
}
