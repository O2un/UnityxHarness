using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/RangedSkillData")]
    public sealed class RangedSkillData : ScriptableObject
    {
        [SerializeField, Min(0)] private int _damage = 1;
        [SerializeField, Min(0f)] private float _cooldown = 2f;
        [SerializeField, Min(0f)] private float _maxCastTime = 1f;
        [SerializeField, Min(0f)] private float _projectileSpeed = 10f;
        [SerializeField, Min(0f)] private float _lifetime = 2f;
        [SerializeField] private Projectile2DView _projectilePrefab;
        [SerializeField] private string _poolKey;
        [SerializeField] private Vector2 _muzzleOffset;

        public int Damage => _damage;
        public float Cooldown => _cooldown;
        public float MaxCastTime => _maxCastTime;
        public float ProjectileSpeed => _projectileSpeed;
        public float Lifetime => _lifetime;
        public Projectile2DView ProjectilePrefab => _projectilePrefab;
        public string PoolKey => true == string.IsNullOrEmpty(_poolKey) && null != _projectilePrefab
            ? _projectilePrefab.name
            : _poolKey;
        public Vector2 MuzzleOffset => _muzzleOffset;
    }
}
