using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/States/Chase", fileName = "ChaseStateSO")]
    public sealed class ChaseStateSO : Enemy2DStateSO
    {
        [SerializeField, Min(0f)] private float _chaseSpeed = 3.5f;

        public override IState Build(Enemy2DAIContext context)
        {
            return new ChaseState(context.Blackboard, context.Mover, _chaseSpeed);
        }
    }
}
