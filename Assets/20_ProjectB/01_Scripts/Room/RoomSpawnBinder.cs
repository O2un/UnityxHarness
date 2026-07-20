using System;
using O2un.Manager;
using R3;
using VContainer.Unity;

namespace O2un.ProjectB.Platformer
{
    public sealed class RoomSpawnBinder : IStartable, IDisposable
    {
        private readonly IEnemySpawner _spawner;
        private readonly IRoomSignalPublisher _publisher;
        private readonly IRoomSignalSource _source;

        private readonly CompositeDisposable _disposables = new();

        // 룸 씬 로드 await가 풀리기 전(스코프 Awake 시점)에 구독을 끝내야 베이스 쪽 RequestSpawnBegin을 놓치지 않는다.
        public RoomSpawnBinder(
            IEnemySpawner spawner,
            IRoomSignalPublisher publisher,
            IRoomSignalSource source)
        {
            _spawner = spawner;
            _publisher = publisher;
            _source = source;

            _source.OnSpawnBeginRequested
                .Subscribe(_ => _spawner.Begin())
                .AddTo(_disposables);

            _spawner.OnCleared
                .Subscribe(_ => _publisher.PublishRoomCleared())
                .AddTo(_disposables);
        }

        public void Start()
        {
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
