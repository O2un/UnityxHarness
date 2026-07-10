using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public abstract class EnemyAIProfileSO : ScriptableObject
    {
        public abstract BaseEnemyAI Build(EnemyBlackboard blackboard, CharacterMover mover);
    }
}
