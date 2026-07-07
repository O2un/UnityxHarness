using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/Chase AI Profile", fileName = "ChaseAIProfile")]
    public sealed class ChaseAIProfile : ScriptableObject
    {
        public BaseEnemyAI Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            SeekPlayerState seek = new(blackboard, mover);
            return new ChaseEnemyAI(seek);
        }
    }
}
