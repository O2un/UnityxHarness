using O2un.Actors;
using O2un.Combat;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class EnemyMeleeAttackView : MonoBehaviour, IEnemyAttackExecutor
    {
        [SerializeField] private EnemyAttackData _data;
        [SerializeField] private MeleeAnimationEventBridge _bridge;
        [SerializeField] private SpriteRenderer _facingRenderer;
        [SerializeField] private string[] _attackStateNames = { "attack1", "attack2" };

        private readonly Subject<Unit> _onAttackAnimationEnd = new();
        private readonly CompositeDisposable _disposables = new();

        private BoxCollider2D _collider;
        private Animator _animator;
        private HitboxModule _hitbox;
        private bool _isAttacking;

        private BoxCollider2D Collider => _collider ??= GetComponent<BoxCollider2D>();
        private Animator Animator => _animator ??= GetComponentInParent<Animator>();

        public Observable<Unit> OnAttackAnimationEnd => _onAttackAnimationEnd;
        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            Collider.isTrigger = true;
            Collider.enabled = false;

            _hitbox = new HitboxModule(new HitboxConfig(
                null != _data ? _data.Damage : 1,
                ActorType.Player,
                HitPolicy.OncePerTarget,
                0f,
                float.MaxValue));

            _hitbox.OnHit.Subscribe(e => e.Target.ApplyDamage(e.Damage)).AddTo(_disposables);

            if (null == _bridge)
            {
                return;
            }

            _bridge.OnHitboxOn.Subscribe(_ => EnableHitbox()).AddTo(_disposables);
            _bridge.OnHitboxOff.Subscribe(_ => DisableHitbox()).AddTo(_disposables);
            _bridge.OnAttackEnd.Subscribe(_ => OnAnimationEnd()).AddTo(_disposables);
        }

        public void PlayAttack()
        {
            if (null == Animator || 0 == _attackStateNames.Length)
            {
                _onAttackAnimationEnd.OnNext(Unit.Default);
                return;
            }

            _isAttacking = true;
            int index = Random.Range(0, _attackStateNames.Length);
            Animator.Play(_attackStateNames[index], 0, 0f);
        }

        private void EnableHitbox()
        {
            if (null == _data)
            {
                return;
            }

            bool facingLeft = null != _facingRenderer && true == _facingRenderer.flipX;
            Collider.size = _data.HitboxSize;
            Collider.offset = new Vector2(
                true == facingLeft ? -_data.HitboxOffset.x : _data.HitboxOffset.x,
                _data.HitboxOffset.y);
            Collider.enabled = true;
            _hitbox.Reset();
        }

        private void DisableHitbox()
        {
            Collider.enabled = false;
        }

        private void OnAnimationEnd()
        {
            DisableHitbox();
            _isAttacking = false;
            _onAttackAnimationEnd.OnNext(Unit.Default);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryHit(other);
        }

        private void TryHit(Collider2D other)
        {
            if (false == Collider.enabled)
            {
                return;
            }

            Damageable2DView target = other.GetComponentInParent<Damageable2DView>();
            if (null == target)
            {
                return;
            }

            _hitbox.TryHit(target);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _onAttackAnimationEnd.Dispose();
        }
    }
}
