using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/States/Attack", fileName = "AttackStateSO")]
    public sealed class AttackStateSO : Enemy2DStateSO
    {
        [SerializeField] private EnemyAttackData _attackData;

        public override IState Build(Enemy2DAIContext context)
        {
            IAttackHook hook = new AnimatedAttackHook(context.AttackExecutor, _attackData.Cooldown);
            return new AttackState(context.Blackboard, context.Mover, hook, _attackData.AttackRange);
        }
    }
}
