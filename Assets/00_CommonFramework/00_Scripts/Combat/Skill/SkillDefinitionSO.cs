using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public abstract class SkillDefinitionSO : ScriptableObject
    {
        [SerializeField] protected float _cooldown = 1f;
        [SerializeField] protected int _damage = 1;
        [SerializeField] protected float _lifetime = 0.3f;
        [SerializeField] protected AttackHitboxView _hitboxPrefab;
        [SerializeField] protected string _poolKey;
        [SerializeField] protected ActorType _targetTeam = ActorType.Enemy;

        protected string PoolKey =>
            string.IsNullOrEmpty(_poolKey)
                ? (null != _hitboxPrefab ? _hitboxPrefab.name : name)
                : _poolKey;

        public abstract ISkillDefinition Build();
    }
}
