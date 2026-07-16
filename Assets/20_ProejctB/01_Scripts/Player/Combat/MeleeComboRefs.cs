using System;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [Serializable]
    public sealed class MeleeComboRefs
    {
        [SerializeField] private MeleeComboData _data;
        [SerializeField] private MeleeAttackView _attackView;
        [SerializeField] private MeleeAnimationEventBridge _bridge;

        public MeleeComboData Data => _data;
        public MeleeAttackView AttackView => _attackView;
        public MeleeAnimationEventBridge Bridge => _bridge;

        public bool IsValid => null != _data && null != _attackView && null != _bridge;
    }
}
