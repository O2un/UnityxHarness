using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public abstract class EnemyStateSO : ScriptableObject
    {
        public abstract IState Build(EnemyBlackboard blackboard, CharacterMover mover);
    }
}
