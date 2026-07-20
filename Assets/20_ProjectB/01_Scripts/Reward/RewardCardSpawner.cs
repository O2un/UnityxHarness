using System.Collections.Generic;
using O2un.DI;
using O2un.Manager;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    public sealed class RewardCardSpawner : MonoBehaviour, ISceneInitializable
    {
        private const string POOL_KEY = "reward_card";

        [SerializeField] private RewardCardView _cardPrefab;
        // 문 콜라이더 반폭 1.25 + 카드 반폭 0.75 = 2.0을 넘겨야 문과 겹치지 않는다
        [SerializeField, Min(0f)] private float _spreadX = 3f;
        [SerializeField] private Color _statTint = new(0.35f, 0.7f, 1f);
        [SerializeField] private Color _passiveTint = new(1f, 0.75f, 0.3f);

        private IPoolService _pool;
        private RoomRewardModule _reward;
        private IRoomSignalSource _signals;

        private readonly List<RewardCardView> _active = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly CompositeDisposable _spawnDisposables = new();

        private Vector3 _spawnPosition;
        private bool _hasSpawnPoint;

        [Inject]
        public void Construct(IPoolService pool, RoomRewardModule reward, IRoomSignalSource signals)
        {
            _pool = pool;
            _reward = reward;
            _signals = signals;
        }

        public void Init()
        {
            if (null == _cardPrefab)
            {
                Debug.LogError($"[RewardCardSpawner] '{name}' _cardPrefab이 비어 있습니다. 보상 카드가 스폰되지 않습니다.");
                return;
            }

            _signals.OnRewardSpawnPointPublished
                .Subscribe(point =>
                {
                    _spawnPosition = point.Position;
                    _hasSpawnPoint = point.HasPoint;
                })
                .AddTo(_disposables);

            _reward.OnCandidatesReady.Subscribe(Spawn).AddTo(_disposables);
            _reward.OnCandidatesCleared.Subscribe(_ => ReleaseAll()).AddTo(_disposables);
            _reward.OnSlotsFull.Subscribe(_ => ShowSlotsFullNotice()).AddTo(_disposables);
        }

        private void Spawn(IReadOnlyList<UpgradeCardSO> candidates)
        {
            ReleaseAll();

            if (false == _hasSpawnPoint || 0 == candidates.Count)
            {
                return;
            }

            // 풀 루트를 이 오브젝트(ProjectB 씬)로 두면 룸 언로드로 카드 인스턴스가 파괴되지 않는다.
            if (false == _pool.IsRegistered(POOL_KEY))
            {
                _pool.Register(POOL_KEY, _cardPrefab, transform);
            }

            IPoolHandle<RewardCardView> handle = _pool.GetHandle<RewardCardView>(POOL_KEY);
            if (null == handle)
            {
                return;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                UpgradeCardSO card = candidates[i];
                RewardCardView view = handle.Get();

                view.Bind(card, ResolvePosition(i, candidates.Count), ResolveTint(card));
                view.OnSelectRequested
                    .Subscribe(selected => _reward.RequestSelect(selected))
                    .AddTo(_spawnDisposables);

                _active.Add(view);
            }
        }

        private Vector3 ResolvePosition(int index, int count)
        {
            if (1 == count)
            {
                return _spawnPosition;
            }

            float t = count > 1 ? index / (float)(count - 1) : 0.5f;
            return _spawnPosition + new Vector3(Mathf.Lerp(-_spreadX, _spreadX, t), 0f, 0f);
        }

        private Color ResolveTint(UpgradeCardSO card)
        {
            return UpgradeCardKind.PassiveSkill == card.Kind ? _passiveTint : _statTint;
        }

        private void ShowSlotsFullNotice()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                _active[i].ShowSlotsFullNotice(true);
            }
        }

        private void ReleaseAll()
        {
            _spawnDisposables.Clear();

            for (int i = 0; i < _active.Count; i++)
            {
                _active[i].ReleaseSelf();
            }

            _active.Clear();
        }

        private void OnDestroy()
        {
            _spawnDisposables.Dispose();
            _disposables.Dispose();
        }
    }
}
