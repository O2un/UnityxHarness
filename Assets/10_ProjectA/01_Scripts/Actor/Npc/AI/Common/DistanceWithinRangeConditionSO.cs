using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/Conditions/Distance Within Range", fileName = "DistanceWithinRangeConditionSO")]
    public sealed class DistanceWithinRangeConditionSO : EnemyTransitionConditionSO
    {
        [SerializeField] private float _range;

        public override ITransitionCondition Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            return new DistanceWithinRangeCondition(blackboard, _range);
        }
    }
}
