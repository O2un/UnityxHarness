using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public abstract class EnemyTransitionConditionSO : ScriptableObject
    {
        public abstract ITransitionCondition Build(EnemyBlackboard blackboard, CharacterMover mover);
    }
}
