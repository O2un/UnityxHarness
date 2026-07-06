using System.Threading;
using Cysharp.Threading.Tasks;
using O2un.Actors;
using O2un.Manager;
using UnityEngine;
using VContainer;

namespace O2un.Dev
{
    /// <summary>
    /// PoolManager 동작 확인용 데모. 일정 간격으로 PlayerContext 를 격자로 스폰하고
    /// 잠시 뒤 풀에 반환한다. 개발 검증 전용.
    /// </summary>
    public sealed class PoolDemo : MonoBehaviour
    {
        private const string POOL_KEY = "Test";
        private const int RELEASE_DELAY_MS = 3000;

        [Inject] private IPoolService _poolService;

        [SerializeField] private EnemyContext _prefab;
        [SerializeField] private float _interval = 1f;
        [SerializeField] private int _row = 3;
        [SerializeField] private float _gap = 0.5f;

        private IPoolHandle<EnemyContext> _pool;
        private int _col = 0;
        private float _elapsed = 0f;
        private CancellationTokenSource _cts;

        private void Start()
        {
            _cts = new CancellationTokenSource();

            if (false == _poolService.IsRegistered(POOL_KEY))
            {
                _poolService.Register(POOL_KEY, _prefab);
            }

            _pool = _poolService.GetHandle<EnemyContext>(POOL_KEY);
        }

        private void Update()
        {
            if (null == _pool)
            {
                return;
            }

            _elapsed += Time.deltaTime;

            if (_elapsed < _interval)
            {
                return;
            }

            SpawnRow();

            _col = (_col + 1) % 10;
            _elapsed = 0f;
        }

        private void SpawnRow()
        {
            for (int i = 0; i < _row; ++i)
            {
                EnemyContext actor = _pool.Get();
                actor.transform.position = new Vector3(i * _gap, 0f, _col * _gap);
                ReleaseAfterDelay(actor, _cts.Token).Forget();
            }
        }

        private async UniTask ReleaseAfterDelay(EnemyContext actor, CancellationToken token)
        {
            bool canceled = await UniTask.Delay(RELEASE_DELAY_MS, cancellationToken: token)
                .SuppressCancellationThrow();

            if (true == canceled)
            {
                return;
            }

            actor.Release();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
