using System;
using VContainer;
using VContainer.Unity;

namespace O2un.Manager
{
    public static class EnemySpawnerRegistration
    {
        public static RegistrationBuilder RegisterEnemySpawner(this IContainerBuilder builder, WaveDataSO waveData)
        {
            return builder.RegisterEnemySpawner<DefaultSpawnPlacer>(waveData);
        }

        public static RegistrationBuilder RegisterEnemySpawner<TPlacer>(
            this IContainerBuilder builder,
            WaveDataSO waveData)
            where TPlacer : ISpawnPlacer
        {
            builder.Register<TPlacer>(Lifetime.Singleton).As<ISpawnPlacer>();

            RegistrationBuilder spawner = builder.Register<EnemySpawnManager>(Lifetime.Singleton)
                    .WithParameter(waveData)
                    .As<IEnemySpawner>()
                    .As<IAsyncStartable>()
                    .As<IDisposable>();

            if (SpawnTriggerMode.KillBased != waveData.TriggerMode)
            {
                spawner.As<ITickable>();
            }

            return spawner;
        }
    }
}
