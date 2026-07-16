using O2un.Combat;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MeleeAttackView : MonoBehaviour
    {
        // BaseBattleController(서드파티)의 스테이지별 트리거 이름에 맞춘다
        private static readonly int[] _attackHashes =
        {
            Animator.StringToHash("Attack1"),
            Animator.StringToHash("Attack2"),
            Animator.StringToHash("Attack3"),
        };

        [SerializeField] private SpriteRenderer _facingRenderer;
        [SerializeField] private string _cancelStateName = "Stand";

        private BoxCollider2D _collider;
        private Animator _animator;
        private HitboxModule _hitbox;

        private BoxCollider2D Collider => _collider ??= GetComponent<BoxCollider2D>();
        private Animator Animator => _animator ??= GetComponentInParent<Animator>();

        private void Awake()
        {
            Collider.isTrigger = true;
            Collider.enabled = false;
        }

        public void Configure(HitboxModule hitbox)
        {
            _hitbox = hitbox;
        }

        public void PlayAttack(int stage)
        {
            Animator.SetTrigger(_attackHashes[stage - 1]);
        }

        public void PlayCancel()
        {
            foreach (int hash in _attackHashes)
            {
                Animator.ResetTrigger(hash);
            }

            Animator.CrossFade(_cancelStateName, 0.05f);
        }

        public void EnableHitbox(Vector2 size, Vector2 offset)
        {
            bool facingLeft = null != _facingRenderer && true == _facingRenderer.flipX;
            Collider.size = size;
            Collider.offset = new Vector2(true == facingLeft ? -offset.x : offset.x, offset.y);
            Collider.enabled = true;
            _hitbox?.Reset();
        }

        public void DisableHitbox()
        {
            Collider.enabled = false;
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
            if (false == Collider.enabled || null == _hitbox)
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
    }
}
