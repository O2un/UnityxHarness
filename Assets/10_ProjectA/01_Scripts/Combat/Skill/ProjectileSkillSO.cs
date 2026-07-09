using UnityEngine;

namespace O2un.Combat
{
    [CreateAssetMenu(menuName = "O2un/Combat/Projectile Skill", fileName = "ProjectileSkill")]
    public sealed class ProjectileSkillSO : SkillDefinitionSO
    {
        [SerializeField] private float _speed = 8f;

        public override ISkillDefinition Build()
        {
            return new ProjectileSkill(
                _skillId,
                BuildStats(speed: _speed),
                new NearestEnemyStrategy());
        }
    }
}
