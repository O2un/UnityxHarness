using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public abstract class Enemy2DTransitionConditionSO : ScriptableObject
    {
        public abstract ITransitionCondition Build(Enemy2DAIContext context, IState fromState);
    }
}
