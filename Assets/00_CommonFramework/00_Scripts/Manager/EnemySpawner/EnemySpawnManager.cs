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
    public sealed class EnemySpawnManager : IEnemySpawner, IAsyncStartable, ITickable, IDisposable
    {
        private readonly IAssetService _assetService;
        private readonly IPoolService _poolService;
        private readonly IActorQuery _actorQuery;
        private readonly IEnemyKillEvent _killEvent;
        private readonly ISpawnPlacer _spawnPlacer;

        public EnemySpawnManager(
            IAssetService assetService,
            IPoolService poolService,
            IActorQuery actorQuery,
            IEnemyKillEvent killEvent,
            ISpawnPlacer spawnPlacer,
            WaveDataSO waveData)
        {
            _assetService = assetService;
            _poolService = poolService;
            _actorQuery = actorQuery;
            _killEvent = killEvent;
            _spawnPlacer = spawnPlacer;

            if (SpawnTriggerMode.KillBased == waveData.TriggerMode)
            {
                _killWaveModule = new KillWaveModule(waveData.Waves);
            }
            else
            {
                _waveModule = new WaveModule(waveData.Waves);
            }

            _killSubscription = _killEvent.OnKilled.Subscribe(_ => OnEnemyKilled());
        }

        private readonly WaveModule _waveModule;
        private readonly KillWaveModule _killWaveModule;
        private readonly System.Random _placementRng = new();
        private readonly Subject<Unit> _onCleared = new();
        private readonly IDisposable _killSubscription;

        private bool _ready;
        private bool _running;
        private bool _cleared;
        private bool _killWavesStarted;
        private float _timer;
        private int _activeCount;

        public Observable<Unit> OnCleared => _onCleared;

        public int ReachedWave => null != _killWaveModule ? _killWaveModule.ReachedWave : _waveModule.ReachedWave;
        public int TotalWaves => null != _killWaveModule ? _killWaveModule.TotalWaves : _waveModule.TotalWaves;

        private IReadOnlyList<string> RequiredKeys =>
                null != _killWaveModule ? _killWaveModule.RequiredKeys : _waveModule.RequiredKeys;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            try
            {
                foreach (string key in RequiredKeys)
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
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                // 여기서 멈추면 KillBased는 클리어 신호도 없이 무징후로 정지하므로 반드시 진단을 남긴다.
                Debug.LogError($"[EnemySpawnManager] 적 프리팹 로드에 실패해 스폰을 시작하지 못했습니다: {exception}");
                return;
            }

            _ready = true;
            TryStartKillWaves();
        }

        public void Begin()
        {
            _running = true;
            TryStartKillWaves();
        }

        public void Tick()
        {
            if (null != _killWaveModule)
            {
                return;
            }

            if (false == _ready || false == _running)
            {
                return;
            }

            _timer += Time.deltaTime;

            IReadOnlyList<SpawnRequest> spawns = _waveModule.GetSpawnsAt(_timer);
            _activeCount += SpawnAll(spawns);

            EvaluateClear();
        }

        private int SpawnAll(IReadOnlyList<SpawnRequest> spawns)
        {
            int spawned = 0;
            for (int i = 0; i < spawns.Count; i++)
            {
                SpawnRequest request = spawns[i];
                IPoolHandle<EnemyContext> handle = _poolService.GetHandle<EnemyContext>(request.Key);
                if (null == handle)
                {
                    continue;
                }

                EnemyContext enemy = handle.Get();
                _spawnPlacer.Place(enemy, ResolvePosition(request));
                spawned++;
            }

            return spawned;
        }

        private void TryStartKillWaves()
        {
            if (null == _killWaveModule || true == _killWavesStarted)
            {
                return;
            }

            if (false == _ready || false == _running)
            {
                return;
            }

            _killWavesStarted = true;
            AdvanceKillWaves();
        }

        private void AdvanceKillWaves()
        {
            while (_killWaveModule.TryAdvance(out IReadOnlyList<SpawnRequest> spawns))
            {
                int spawned = SpawnAll(spawns);
                _killWaveModule.NotifySpawned(spawned);
                if (0 < spawned)
                {
                    return;
                }
            }

            EmitCleared();
        }

        private void OnEnemyKilled()
        {
            if (null != _killWaveModule)
            {
                OnKillBasedEnemyKilled();
                return;
            }

            if (0 < _activeCount)
            {
                _activeCount--;
            }

            EvaluateClear();
        }

        private void OnKillBasedEnemyKilled()
        {
            if (false == _killWavesStarted || true == _cleared)
            {
                return;
            }

            _killWaveModule.NotifyKilled();
            if (false == _killWaveModule.IsCurrentWaveCleared)
            {
                return;
            }

            AdvanceKillWaves();
        }

        private void EvaluateClear()
        {
            if (false == _waveModule.IsExhausted || 0 < _activeCount)
            {
                return;
            }

            EmitCleared();
        }

        private void EmitCleared()
        {
            if (true == _cleared)
            {
                return;
            }

            _cleared = true;
            _onCleared.OnNext(Unit.Default);
        }

        public void Reset()
        {
            _waveModule?.Reset();
            _killWaveModule?.Reset();
            _running = false;
            _killWavesStarted = false;
            _timer = 0f;
            _activeCount = 0;
            _cleared = false;
        }

        public void Dispose()
        {
            _killSubscription.Dispose();
            _onCleared.Dispose();
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
