using UnityEngine;

namespace O2un.Combat
{
    [CreateAssetMenu(menuName = "O2un/Combat/Aura Field Skill", fileName = "AuraFieldSkill")]
    public sealed class AuraFieldSkillSO : SkillDefinitionSO
    {
        [SerializeField] private float _reHitInterval = 0.5f;

        public override ISkillDefinition Build()
        {
            return new AuraFieldSkill(
                _skillId,
                BuildStats(reHitInterval: _reHitInterval),
                new SelfTargetStrategy());
        }
    }
}
