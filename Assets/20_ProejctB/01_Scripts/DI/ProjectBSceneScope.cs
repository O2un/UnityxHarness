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
        [SerializeField] private UpgradeCardPoolSO _upgradeCardPool;

        [SerializeField] private List<GameObject> _sceneInitializables = new();

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ActorManager>().AsSelf().As<IActorRegistry>().As<IActorQuery>();

            builder.Register<PoolManager>(Lifetime.Singleton).As<IPoolService>();
            builder.Register<PlayerDataStore>(Lifetime.Singleton).AsImplementedInterfaces();
            // 룸을 넘어 스탯이 유지되어야 하므로 RoomSceneScope가 아닌 여기에 등록한다
            builder.Register<PlayerStatModule>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayerSkillStatusStore>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<InventoryManager>(Lifetime.Singleton).AsImplementedInterfaces();
            // 슬롯 구독이 시작되려면 즉시 생성돼야 하므로 지연 생성되는 Register 대신 EntryPoint로 올린다
            builder.RegisterEntryPoint<UpgradeStatAggregator>().As<IPassiveSkillQuery>();

            if (null == _upgradeCardPool)
            {
                Debug.LogError($"[ProjectBSceneScope] '{name}' _upgradeCardPool이 비어 있습니다. 강화 카드 획득이 등록되지 않습니다.");
            }
            else
            {
                // UpgradeCardPoolSO 소비처가 UpgradeCardAcquisition 하나뿐이라 전역 등록 대신 파라미터로 넘긴다.
                builder.Register<UpgradeCardAcquisition>(Lifetime.Singleton)
                        .WithParameter(_upgradeCardPool)
                        .AsSelf()
                        .As<IDisposable>();
            }

            builder.RegisterEntryPoint<RoomRewardModule>().AsSelf();

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
