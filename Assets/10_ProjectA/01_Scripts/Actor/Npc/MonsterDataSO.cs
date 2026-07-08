using O2un.Combat;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/Actor/Monster Data", fileName = "MonsterData")]
    public sealed class MonsterDataSO : ScriptableObject
    {
        [SerializeField] private int _maxHp = 10;
        [SerializeField] private MoveStats _move;
        [SerializeField] private SkillDefinitionSO _attackSkill;

        public int MaxHp => _maxHp;
        public MoveStats Move => _move;
        public SkillDefinitionSO AttackSkill => _attackSkill;
    }
}
