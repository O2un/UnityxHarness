using System.Collections.Generic;
using O2un.DI;
using O2un.Manager;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace O2un.ProjectB.Platformer
{
    public class RoomSceneScope : LifetimeScope
    {
        [SerializeField] private WaveDataSO _waveData;
        [SerializeField] private Transform _playerSpawnPoint;

        [SerializeField] private List<GameObject> _sceneInitializables = new();

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
    }
}
