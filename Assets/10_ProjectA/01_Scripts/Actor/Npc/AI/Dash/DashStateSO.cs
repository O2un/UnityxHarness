using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/States/Dash State", fileName = "DashStateSO")]
    public sealed class DashStateSO : EnemyStateSO
    {
        [SerializeField] private float _dashSpeed;
        [SerializeField] private float _dashDistance;
        [SerializeField] private int _contactDamage;
        [SerializeField] private float _contactRadius = 1f;

        public override IState Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            return new DashState(blackboard, mover, _dashSpeed, _dashDistance, _contactDamage, _contactRadius);
        }
    }
}
