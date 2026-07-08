using UnityEngine;

namespace O2un.Combat
{
    [CreateAssetMenu(menuName = "O2un/Combat/Melee Swing Skill", fileName = "MeleeSwingSkill")]
    public sealed class MeleeSwingSkillSO : SkillDefinitionSO
    {
        [SerializeField] private float _range = 2f;

        public override ISkillDefinition Build()
        {
            return new MeleeSwingSkill(
                _cooldown,
                _range,
                _lifetime,
                _damage,
                _targetTeam,
                _hitboxPrefab,
                PoolKey,
                new NearestEnemyStrategy());
        }
    }
}
