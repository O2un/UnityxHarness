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

        public Observable<Unit> OnSpawnBeginRequested => _onSpawnBeginRequested;
        public Observable<Unit> OnRoomCleared => _onRoomCleared;
        public Observable<Vector3> OnRoomReady => _onRoomReady;

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

        public void Dispose()
        {
            _onSpawnBeginRequested.Dispose();
            _onRoomCleared.Dispose();
            _onRoomReady.Dispose();
        }
    }
}
