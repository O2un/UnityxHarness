using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace O2un.Manager
{
    public sealed class EnemySpawnManager : IAsyncStartable, ITickable
    {
        private readonly IAssetService _assetService;
        private readonly IPoolService _poolService;
        private readonly WaveModule _waveModule;

        public EnemySpawnManager(IAssetService assetService, IPoolService poolService, WaveDataSO waveData)
        {
            _assetService = assetService;
            _poolService = poolService;
            _waveModule = new WaveModule(waveData.Waves);
        }

        private bool _ready;
        private float _timer;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            foreach (string key in _waveModule.RequiredKeys)
            {
                GameObject prefab = await _assetService.LoadAsync<GameObject>(key);
                EnemyContext context = prefab.GetComponent<EnemyContext>();
                if (null == context)
                {
                    Debug.LogError($"[EnemySpawnManager] '{key}' 프리팹에 EnemyContext가 없습니다.");
                    continue;
                }

                if (false == _poolService.IsRegistered(key))
                {
                    _poolService.Register(key, context);
                }
            }

            _ready = true;
        }

        public void Tick()
        {
            if (false == _ready)
            {
                return;
            }

            _timer += Time.deltaTime;

            IReadOnlyList<SpawnRequest> spawns = _waveModule.GetSpawnsAt(_timer);
            for (int i = 0; i < spawns.Count; i++)
            {
                SpawnRequest request = spawns[i];
                IPoolHandle<EnemyContext> handle = _poolService.GetHandle<EnemyContext>(request.Key);
                if (null == handle)
                {
                    continue;
                }

                EnemyContext enemy = handle.Get();
                enemy.transform.position = request.Position;
            }
        }
    }
}
