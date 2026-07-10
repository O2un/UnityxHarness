using UnityEngine;

namespace O2un.Combat
{
    [CreateAssetMenu(menuName = "O2un/Combat/Enemy Melee Swing Skill", fileName = "EnemyMeleeSwingSkill")]
    public sealed class EnemyMeleeSwingSkillSO : SkillDefinitionSO
    {
        [SerializeField] private float _range = 2.5f;

        public override ISkillDefinition Build()
        {
            return new MeleeSwingSkill(
                _skillId,
                BuildStats(range: _range),
                new NearestPlayerStrategy());
        }
    }
}
