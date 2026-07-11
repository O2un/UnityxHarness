using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using O2un.Actors;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace O2un.Manager
{
    public sealed class EnemySpawnManager : IAsyncStartable, ITickable, IDisposable
    {
        private readonly IAssetService _assetService;
        private readonly IPoolService _poolService;
        private readonly IActorQuery _actorQuery;
        private readonly IEnemyKillEvent _killEvent;
        private readonly WaveModule _waveModule;
        private readonly System.Random _placementRng = new();
        private readonly Subject<Unit> _onCleared = new();
        private readonly IDisposable _killSubscription;

        public EnemySpawnManager(
            IAssetService assetService,
            IPoolService poolService,
            IActorQuery actorQuery,
            IEnemyKillEvent killEvent,
            WaveDataSO waveData)
        {
            _assetService = assetService;
            _poolService = poolService;
            _actorQuery = actorQuery;
            _killEvent = killEvent;
            _waveModule = new WaveModule(waveData.Waves);
            _killSubscription = _killEvent.OnKilled.Subscribe(_ => OnEnemyKilled());
        }

        private bool _ready;
        private bool _running;
        private bool _cleared;
        private float _timer;
        private int _activeCount;

        public Observable<Unit> OnCleared => _onCleared;
        public bool IsCleared => _cleared;
        public int ReachedWave => _waveModule.ReachedWave;
        public int TotalWaves => _waveModule.TotalWaves;

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

        public void Begin()
        {
            _running = true;
        }

        public void Tick()
        {
            if (false == _ready || false == _running)
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
                _activeCount++;
            }

            EvaluateClear();
        }

        private void OnEnemyKilled()
        {
            if (0 < _activeCount)
            {
                _activeCount--;
            }

            EvaluateClear();
        }

        private void EvaluateClear()
        {
            if (true == _cleared)
            {
                return;
            }

            if (false == _waveModule.IsExhausted || 0 < _activeCount)
            {
                return;
            }

            _cleared = true;
            _onCleared.OnNext(Unit.Default);
        }

        public void Reset()
        {
            _waveModule.Reset();
            _running = false;
            _timer = 0f;
            _activeCount = 0;
            _cleared = false;
        }

        public void Dispose()
        {
            _killSubscription.Dispose();
            _onCleared.Dispose();
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
            target.SetPositionAndRotation(position, Quaternion.identity);
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
