using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public interface IRoomSignalPublisher
    {
        void RequestSpawnBegin();
        void PublishRoomCleared();
        void PublishRoomReady(Vector3 playerSpawnPosition);
        void PublishRewardSpawnPoint(Vector3 position, bool hasPoint);
    }

    public interface IRoomSignalSource
    {
        Observable<Unit> OnSpawnBeginRequested { get; }
        Observable<Unit> OnRoomCleared { get; }
        Observable<Vector3> OnRoomReady { get; }
        Observable<RewardSpawnPoint> OnRewardSpawnPointPublished { get; }
    }

    public readonly struct RewardSpawnPoint
    {
        public Vector3 Position { get; }
        public bool HasPoint { get; }

        public RewardSpawnPoint(Vector3 position, bool hasPoint)
        {
            Position = position;
            HasPoint = hasPoint;
        }
    }
}
