using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using O2un.Actors;
using UnityEngine;
using VContainer.Unity;

namespace O2un.Manager
{
    public sealed class EnemySpawnManager : IAsyncStartable, ITickable
    {
        private readonly IAssetService _assetService;
        private readonly IPoolService _poolService;
        private readonly IActorQuery _actorQuery;
        private readonly WaveModule _waveModule;
        private readonly System.Random _placementRng = new();

        public EnemySpawnManager(
            IAssetService assetService,
            IPoolService poolService,
            IActorQuery actorQuery,
            WaveDataSO waveData)
        {
            _assetService = assetService;
            _poolService = poolService;
            _actorQuery = actorQuery;
            _waveModule = new WaveModule(waveData.Waves);
        }

        private bool _ready;
        private float _timer;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (0 <= enemyLayer)
            {
                Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
            }

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
                Teleport(enemy.transform, ResolvePosition(request));
            }
        }

        private static void Teleport(Transform target, Vector3 position)
        {
            CharacterController controller = target.GetComponent<CharacterController>();
            if (null == controller)
            {
                target.position = position;
                return;
            }

            controller.enabled = false;
            target.position = position;
            controller.enabled = true;
        }

        private Vector3 ResolvePosition(in SpawnRequest request)
        {
            if (SpawnPlacement.PlayerRadius != request.Placement)
            {
                return request.Position;
            }

            IActor player = _actorQuery.Player;
            Vector3 center = null != player ? player.Transform.position : Vector3.zero;
            return AnnulusSampler.Sample(center, request.MinRadius, request.MaxRadius, _placementRng);
        }
    }
}
