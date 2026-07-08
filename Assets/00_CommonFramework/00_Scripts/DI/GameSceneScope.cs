using O2un.Actors;
using O2un.Camera;
using O2un.Combat;
using O2un.DataStore;
using O2un.Manager;
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

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CameraManager>(Lifetime.Singleton)
                    .WithParameter("gamePlay", _gamePlay).WithParameter("cinematic", _cinematicCamera)
                    .AsSelf().AsImplementedInterfaces();

            builder.Register<CameraRelativeMoveModule>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<UIStore>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayerDataStore>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<GameManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<DefaultScoreCalculator>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ScoreManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<InventoryManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<ActorManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.Register<PoolManager>(Lifetime.Singleton).As<IPoolService>();

            builder.Register<AttackSpawner>(Lifetime.Singleton).As<IAttackSpawner>();

            builder.RegisterInstance(_waveData);
            builder.RegisterEntryPoint<EnemySpawnManager>();
        }
    }
}
