using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace O2un.Manager
{
    public static class EnemySpawnerRegistration
    {
        public static RegistrationBuilder RegisterEnemySpawner(
            this IContainerBuilder builder,
            WaveDataSO waveData,
            Transform spawnRoot = null)
        {
            return builder.RegisterEnemySpawner<DefaultSpawnPlacer>(waveData, spawnRoot);
        }

        public static RegistrationBuilder RegisterEnemySpawner<TPlacer>(
            this IContainerBuilder builder,
            WaveDataSO waveData,
            Transform spawnRoot = null)
            where TPlacer : ISpawnPlacer
        {
            builder.Register<TPlacer>(Lifetime.Singleton).As<ISpawnPlacer>();

            RegistrationBuilder spawner = builder.Register<EnemySpawnManager>(Lifetime.Singleton)
                    .WithParameter(waveData)
                    .WithParameter("spawnRoot", spawnRoot)
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
