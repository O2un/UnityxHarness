using O2un.Actors;
using O2un.Camera;
using O2un.Combat;
using O2un.DataStore;
using O2un.Manager;
using O2un.Progression;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace O2un.DI
{
    public class GameSceneScope : LifetimeScope
    {
        [SerializeField] private CinemachineCamera _gamePlay;
        [SerializeField] private CinemachineCamera _cinematicCamera;
        [SerializeField] private WaveDataSO _waveData;
        [SerializeField] private ItemDropDataSO _itemDropData;
        [SerializeField] private ExperienceDataSO _experienceData;
        [SerializeField] private LevelUpSkillPoolSO _levelUpSkillPool;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CameraManager>(Lifetime.Singleton)
                    .WithParameter("gamePlay", _gamePlay).WithParameter("cinematic", _cinematicCamera)
                    .AsSelf().AsImplementedInterfaces();

            builder.Register<CameraRelativeMoveModule>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<UIStore>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayerDataStore>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayerHealthAdapter>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.RegisterEntryPoint<GameManager>().AsSelf().As<IGameManager>();

            builder.Register<DefaultScoreCalculator>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ScoreManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<InventoryManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<ActorManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.Register<PoolManager>(Lifetime.Singleton).As<IPoolService>();

            builder.Register<AttackSpawner>(Lifetime.Singleton).As<IAttackSpawner>();

            builder.RegisterInstance(_waveData);
            builder.RegisterEntryPoint<EnemySpawnManager>().AsSelf();

            builder.RegisterInstance(_itemDropData);
            builder.Register<EnemyKillEvent>(Lifetime.Singleton).As<IEnemyKillEvent>();
            builder.Register<ExpGainedChannel>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterEntryPoint<ItemDropContext>();

            builder.RegisterInstance(_experienceData);
            builder.Register<ExperienceModule>(Lifetime.Singleton)
                    .WithParameter("requiredExpCurve", _experienceData.RequiredExpCurve)
                    .AsImplementedInterfaces();
            builder.RegisterEntryPoint<ExperienceGainContext>();

            builder.RegisterInstance(_levelUpSkillPool);
        }
    }
}
