using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/States/Melee Attack State", fileName = "MeleeAttackStateSO")]
    public sealed class MeleeAttackStateSO : EnemyStateSO
    {
        [SerializeField] private float _attackDuration = 0.5f;

        public override IState Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            return new MeleeAttackState(mover, _attackDuration);
        }
    }
}
