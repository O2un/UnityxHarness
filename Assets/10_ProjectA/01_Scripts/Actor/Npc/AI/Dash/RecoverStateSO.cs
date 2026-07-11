using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/States/Recover State", fileName = "RecoverStateSO")]
    public sealed class RecoverStateSO : EnemyStateSO
    {
        [SerializeField] private float _duration;

        public override IState Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            return new RecoverState(mover, _duration);
        }
    }
}
