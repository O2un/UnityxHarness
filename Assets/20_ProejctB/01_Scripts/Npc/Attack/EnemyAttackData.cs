using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/EnemyAttackData")]
    public sealed class EnemyAttackData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _attackRange = 1.5f;
        [SerializeField, Min(0f)] private float _cooldown = 0.8f;

        [SerializeField, Min(0)] private int _damage = 1;
        [SerializeField] private Vector2 _hitboxSize = new(1f, 1f);
        [SerializeField] private Vector2 _hitboxOffset = new(0.6f, 0f);

        public float AttackRange => _attackRange;
        public float Cooldown => _cooldown;

        public int Damage => _damage;
        public Vector2 HitboxSize => _hitboxSize;
        public Vector2 HitboxOffset => _hitboxOffset;
    }
}
