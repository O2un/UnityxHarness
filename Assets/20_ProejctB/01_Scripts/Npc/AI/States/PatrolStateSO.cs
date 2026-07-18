using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/States/Patrol", fileName = "PatrolStateSO")]
    public sealed class PatrolStateSO : Enemy2DStateSO
    {
        [SerializeField, Min(0f)] private float _patrolSpeed = 2f;

        public override IState Build(Enemy2DAIContext context)
        {
            return new PatrolState(context.Blackboard, context.Mover, _patrolSpeed);
        }
    }
}
