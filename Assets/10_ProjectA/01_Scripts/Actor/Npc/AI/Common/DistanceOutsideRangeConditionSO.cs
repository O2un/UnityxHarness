using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/Conditions/Distance Outside Range", fileName = "DistanceOutsideRangeConditionSO")]
    public sealed class DistanceOutsideRangeConditionSO : EnemyTransitionConditionSO
    {
        [SerializeField] private float _range;

        public override ITransitionCondition Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            return new DistanceOutsideRangeCondition(blackboard, _range);
        }
    }
}
