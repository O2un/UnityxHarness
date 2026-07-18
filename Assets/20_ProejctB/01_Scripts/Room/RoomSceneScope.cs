using System.Collections.Generic;
using O2un.DI;
using O2un.Manager;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace O2un.ProjectB.Platformer
{
    public class RoomSceneScope : LifetimeScope
    {
        [SerializeField] private WaveDataSO _waveData;
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private List<RoomDoorView> _doors = new();

        [SerializeField] private List<GameObject> _sceneInitializables = new();

        private readonly CompositeDisposable _disposables = new();

        public Vector3 PlayerSpawnPosition =>
                null != _playerSpawnPoint ? _playerSpawnPoint.position : Vector3.zero;

        protected override void Configure(IContainerBuilder builder)
        {
            if (null == _waveData)
            {
                Debug.LogError($"[RoomSceneScope] '{name}' _waveData가 비어 있습니다. 이 룸은 스폰도 클리어도 하지 않습니다.");
            }
            else
            {
                // 스폰 루트를 이 스코프로 두면 룸 언로드 시 적 인스턴스가 같이 정리된다.
                builder.RegisterEnemySpawner(_waveData, transform);
                builder.RegisterEntryPoint<RoomSpawnBinder>();
            }

            builder.RegisterBuildCallback(InitializeSceneComponents);
        }

        private void InitializeSceneComponents(IObjectResolver resolver)
        {
            if (null == _playerSpawnPoint)
            {
                Debug.LogError($"[RoomSceneScope] '{name}' _playerSpawnPoint가 비어 있습니다. 플레이어가 이 룸으로 옮겨지지 않습니다.");
            }
            else
            {
                resolver.Resolve<IRoomSignalPublisher>().PublishRoomReady(PlayerSpawnPosition);
            }

            BindDoors(resolver);

            for (int i = 0; i < _sceneInitializables.Count; i++)
            {
                GameObject mb = _sceneInitializables[i];
                if (null == mb)
                {
                    Debug.LogError($"[RoomSceneScope] '{name}' _sceneInitializables[{i}]가 비어 있습니다.");
                    continue;
                }

                var initializable = mb.GetComponent<ISceneInitializable>();
                if (null != initializable)
                {
                    resolver.InjectGameObject(mb);
                    initializable.Init();
                }
            }
        }

        // 문은 진행 가능 여부를 판단하지 않는다. 여기서 신호를 그대로 중계하고,
        // 전환 가능 여부·중복 입력 무시는 RoomModule.RequestTransition의 상태 검사에 맡긴다.
        private void BindDoors(IObjectResolver resolver)
        {
            if (0 == _doors.Count)
            {
                return;
            }

            var progression = resolver.Resolve<IRoomProgression>();

            for (int i = 0; i < _doors.Count; i++)
            {
                RoomDoorView door = _doors[i];
                if (null == door)
                {
                    Debug.LogError($"[RoomSceneScope] '{name}' _doors[{i}]가 비어 있습니다.");
                    continue;
                }

                resolver.InjectGameObject(door.gameObject);

                door.OnTransitionRequested
                    .Subscribe(destinationId => progression.RequestTransition(destinationId))
                    .AddTo(_disposables);
            }
        }

        protected override void OnDestroy()
        {
            _disposables.Dispose();
            base.OnDestroy();
        }
    }
}
