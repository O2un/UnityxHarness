using System;
using System.Collections.Generic;
using O2un.Actors;
using O2un.DataStore;
using O2un.DI;
using O2un.Manager;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace O2un.ProjectB.Platformer
{
    public class ProjectBSceneScope : LifetimeScope
    {
        [SerializeField] private StageDataSO _stageData;

        [SerializeField] private List<GameObject> _sceneInitializables = new();

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ActorManager>().AsSelf().As<IActorRegistry>().As<IActorQuery>();

            builder.Register<PoolManager>(Lifetime.Singleton).As<IPoolService>();
            builder.Register<PlayerDataStore>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<EnemyKillEvent>(Lifetime.Singleton).As<IEnemyKillEvent>();
            builder.Register<RoomSignalChannel>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<ScreenFaderView>().As<IScreenFader>();
            builder.RegisterComponentInHierarchy<Player2DPlacer>().As<IPlayerPlacer>();

            if (null == _stageData)
            {
                Debug.LogError($"[ProjectBSceneScope] '{name}' _stageData가 비어 있습니다. 룸 진행이 등록되지 않습니다.");
            }
            else
            {
                // StageDataSO 소비처가 RoomModule 하나뿐이라 전역 등록 대신 파라미터로 넘긴다.
                builder.Register<RoomModule>(Lifetime.Singleton)
                        .WithParameter(_stageData)
                        .WithParameter<LifetimeScope>(this)
                        .As<IRoomProgression>()
                        .AsSelf()
                        .As<IDisposable>();
            }

            builder.RegisterBuildCallback(InitializeSceneComponents);
        }

        private void InitializeSceneComponents(IObjectResolver resolver)
        {
            for (int i = 0; i < _sceneInitializables.Count; i++)
            {
                GameObject mb = _sceneInitializables[i];
                if (null == mb)
                {
                    Debug.LogError($"[ProjectBSceneScope] '{name}' _sceneInitializables[{i}]가 비어 있습니다.");
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
