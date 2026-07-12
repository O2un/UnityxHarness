using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public abstract class SkillDefinitionSO : ScriptableObject
    {
        [SerializeField] protected string _skillId;
        [SerializeField] protected int _level = 1;
        [SerializeField] protected float _cooldown = 1f;
        [SerializeField] protected int _damage = 1;
        [SerializeField] protected float _lifetime = 0.3f;
        [SerializeField] protected AttackHitboxView _hitboxPrefab;
        [SerializeField] protected string _poolKey;
        [SerializeField] protected ActorType _targetTeam = ActorType.Enemy;
        [SerializeField] protected Sprite _icon;

        public string SkillId => _skillId;
        public int Level => _level;
        public Sprite Icon => _icon;

        protected string PoolKey =>
            string.IsNullOrEmpty(_poolKey)
                ? (null != _hitboxPrefab ? _hitboxPrefab.name : name)
                : _poolKey;

        public abstract ISkillDefinition Build();

        protected SkillStats BuildStats(float speed = 0f, float range = 0f, float reHitInterval = 0f)
        {
            return new SkillStats(
                _level,
                _cooldown,
                _damage,
                _lifetime,
                _hitboxPrefab,
                PoolKey,
                _targetTeam,
                speed,
                range,
                reHitInterval);
        }
    }
}
