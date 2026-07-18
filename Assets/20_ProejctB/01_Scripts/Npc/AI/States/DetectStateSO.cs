using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/States/Detect", fileName = "DetectStateSO")]
    public sealed class DetectStateSO : Enemy2DStateSO
    {
        public override IState Build(Enemy2DAIContext context)
        {
            return new DetectState(context.Blackboard, context.Mover);
        }
    }
}
