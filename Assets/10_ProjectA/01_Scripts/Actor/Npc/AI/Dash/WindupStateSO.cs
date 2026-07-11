using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/States/Windup State", fileName = "WindupStateSO")]
    public sealed class WindupStateSO : EnemyStateSO
    {
        [SerializeField] private float _duration;

        public override IState Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            return new WindupState(mover, _duration);
        }
    }
}
