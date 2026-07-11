using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/States/Seek Player State", fileName = "SeekPlayerStateSO")]
    public sealed class SeekPlayerStateSO : EnemyStateSO
    {
        public override IState Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            return new SeekPlayerState(blackboard, mover);
        }
    }
}
