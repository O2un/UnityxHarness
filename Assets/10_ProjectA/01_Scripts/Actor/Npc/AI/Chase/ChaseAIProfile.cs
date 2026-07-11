using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/Chase AI Profile", fileName = "ChaseAIProfile")]
    public sealed class ChaseAIProfile : EnemyAIProfileSO
    {
        [SerializeField] private EnemyStateSO _seekState;

        public override BaseEnemyAI Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            IState seek = _seekState.Build(blackboard, mover);
            return new ChaseEnemyAI(seek);
        }
    }
}
