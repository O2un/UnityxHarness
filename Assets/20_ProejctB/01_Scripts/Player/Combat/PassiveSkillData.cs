using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/PassiveSkillData")]
    public sealed class PassiveSkillData : ScriptableObject
    {
        [Header("CriticalOnHit")]
        [SerializeField, Range(0f, 1f)] private float _criticalChance = 0.25f;
        [SerializeField, Min(1f)] private float _criticalMultiplier = 2f;

        [Header("HomingMissile")]
        [SerializeField, Min(0)] private int _missileDamage = 3;
        [SerializeField, Min(0f)] private float _missileSpeed = 12f;
        [SerializeField, Min(0f)] private float _missileLifetime = 3f;
        [SerializeField, Min(0f)] private float _missileTurnRate = 360f;
        [SerializeField, Min(0f)] private float _missileCooldown = 0.3f;
        [SerializeField] private Projectile2DView _missilePrefab;
        [SerializeField] private string _missilePoolKey = "passive_homing_missile";
        [SerializeField] private Vector2 _missileMuzzleOffset = new(0.5f, 0.5f);

        public float CriticalChance => _criticalChance;
        public float CriticalMultiplier => _criticalMultiplier;

        public int MissileDamage => _missileDamage;
        public float MissileSpeed => _missileSpeed;
        public float MissileLifetime => _missileLifetime;
        public float MissileTurnRate => _missileTurnRate;
        public float MissileCooldown => _missileCooldown;
        public Projectile2DView MissilePrefab => _missilePrefab;
        public string MissilePoolKey => _missilePoolKey;
        public Vector2 MissileMuzzleOffset => _missileMuzzleOffset;
    }
}
