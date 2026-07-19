using O2un.Actors;
using O2un.Combat;
using O2un.DataStore;
using O2un.DI;
using O2un.Feedback;
using O2un.Input;
using O2un.Manager;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private MovementData _data;
        [SerializeField] private PlayerView _view;
        [SerializeField] private MeleeComboRefs _meleeRefs;
        [SerializeField] private RangedSkillRefs _rangedRefs;
        [SerializeField] private Damageable2DView _damageable;
        [SerializeField] private PassiveSkillData _passiveData;

        [Inject] private IInputReader _input;
        [Inject] private IActorRegistry _registry;
        [Inject] private IPoolService _pool;
        [Inject] private IPlayerDataReader _playerReader;
        [Inject] private IPlayerDataWriter _playerWriter;
        [Inject] private IPlayerStatReader _statReader;
        [Inject] private IPlayerStatWriter _statWriter;
        [Inject] private IPassiveSkillQuery _passiveQuery;
        [Inject] private IActorQuery _actorQuery;
        [Inject] private IPlayerSkillStatusWriter _skillStatusWriter;
        [Inject] private IHitFeedbackPublisher _hitPublisher;

        private Player2DActor _actor;
        private PlayerHealthAdapter _health;

        private readonly CompositeDisposable _disposables = new();

        private int _lastMaxHealth;

        public void Init()
        {
            if (null == _input || null == _data || null == _view || null == _registry || null == _statReader || null == _statWriter)
            {
                Debug.LogError($"[Player2DContext] '{name}' 의존성 주입 실패 — input={_input != null}, data={_data != null}, view={_view != null}, registry={_registry != null}, stat={_statReader != null && _statWriter != null}");
                return;
            }

            _statWriter.SetBase(UpgradeStatType.MoveSpeed, _data.MaxMoveSpeed);

            MeleeComboRefs meleeRefs = _meleeRefs;
            if (null == meleeRefs || false == meleeRefs.IsValid)
            {
                // 근접 콤보 씬 설정(SO·View·Bridge 할당) 전에는 공격만 비활성하고 이동·점프는 유지
                Debug.LogWarning($"[Player2DContext] '{name}' MeleeComboRefs 미할당 — 근접 공격 비활성으로 시작");
                meleeRefs = null;
            }

            RangedSkillRefs rangedRefs = _rangedRefs;
            if (null == rangedRefs || false == rangedRefs.IsValid || null == _pool)
            {
                // 스킬 씬 설정(SO·Bridge·프리팹) 전에는 스킬만 비활성하고 이동·근접은 유지
                Debug.LogWarning($"[Player2DContext] '{name}' RangedSkillRefs/IPoolService 미할당 — 원거리 스킬 비활성으로 시작");
                rangedRefs = null;
            }

            PassiveSkillData passiveData = _passiveData;
            if (null == passiveData || null == _passiveQuery)
            {
                // 패시브 데이터·해금 조회 미할당 시 패시브만 비활성하고 이동·공격은 유지
                Debug.LogWarning($"[Player2DContext] '{name}' PassiveSkillData/IPassiveSkillQuery 미할당 — 패시브 스킬 비활성으로 시작");
                passiveData = null;
            }

            _actor = new Player2DActor(_data, _input, _view, _registry, meleeRefs, rangedRefs, _pool, _statReader, passiveData, _passiveQuery, _actorQuery);

            BindSkillStatus();
            InitHealth();
        }

        private void BindSkillStatus()
        {
            ReadOnlyReactiveProperty<float> cooldown = _actor.RangedCooldownNormalized;
            if (null == _skillStatusWriter || null == cooldown)
            {
                return;
            }

            cooldown.Subscribe(_skillStatusWriter.SetRangedCooldownNormalized).AddTo(_disposables);
        }

        private void InitHealth()
        {
            if (null == _damageable || null == _playerReader || null == _playerWriter)
            {
                // Damageable2DView 미할당 시 피격만 비활성하고 이동·공격은 유지
                Debug.LogWarning($"[Player2DContext] '{name}' Damageable2DView/PlayerData 미할당 — 피격 비활성으로 시작");
                return;
            }

            _playerWriter.SetCurrentHP(_playerReader.MaxHP.CurrentValue);
            _health = new PlayerHealthAdapter(_playerReader, _playerWriter);
            _damageable.Bind(ActorType.Player, _health, _hitPublisher);

            _lastMaxHealth = _playerReader.MaxHP.CurrentValue;
            _statWriter.SetBase(UpgradeStatType.MaxHealth, _lastMaxHealth);
            _statReader.MaxHealth.Subscribe(ApplyMaxHealth).AddTo(_disposables);
        }

        private void ApplyMaxHealth(int maxHealth)
        {
            int delta = maxHealth - _lastMaxHealth;
            _lastMaxHealth = maxHealth;

            if (0 == delta)
            {
                return;
            }

            _playerWriter.VaryMaxHP(delta);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _actor?.Dispose();
            _health?.Dispose();
        }
    }
}
