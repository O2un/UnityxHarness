using System;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class RoomSignalChannel : IRoomSignalPublisher, IRoomSignalSource, IDisposable
    {
        private readonly Subject<Unit> _onSpawnBeginRequested = new();
        private readonly Subject<Unit> _onRoomCleared = new();
        private readonly Subject<Vector3> _onRoomReady = new();
        private readonly Subject<RewardSpawnPoint> _onRewardSpawnPointPublished = new();

        public Observable<Unit> OnSpawnBeginRequested => _onSpawnBeginRequested;
        public Observable<Unit> OnRoomCleared => _onRoomCleared;
        public Observable<Vector3> OnRoomReady => _onRoomReady;
        public Observable<RewardSpawnPoint> OnRewardSpawnPointPublished => _onRewardSpawnPointPublished;

        public void RequestSpawnBegin()
        {
            _onSpawnBeginRequested.OnNext(Unit.Default);
        }

        public void PublishRoomCleared()
        {
            _onRoomCleared.OnNext(Unit.Default);
        }

        public void PublishRoomReady(Vector3 playerSpawnPosition)
        {
            _onRoomReady.OnNext(playerSpawnPosition);
        }

        // 룸마다 반드시 한 번 발행한다. 지점이 없는 룸도 hasPoint=false로 알려야
        // 이전 룸의 지점이 남아 카드가 엉뚱한 곳에 스폰되지 않는다.
        public void PublishRewardSpawnPoint(Vector3 position, bool hasPoint)
        {
            _onRewardSpawnPointPublished.OnNext(new RewardSpawnPoint(position, hasPoint));
        }

        public void Dispose()
        {
            _onSpawnBeginRequested.Dispose();
            _onRoomCleared.Dispose();
            _onRoomReady.Dispose();
            _onRewardSpawnPointPublished.Dispose();
        }
    }
}
