using System;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [Serializable]
    public sealed class RangedSkillRefs
    {
        [SerializeField] private RangedSkillData _data;
        [SerializeField] private RangedAnimationEventBridge _bridge;

        public RangedSkillData Data => _data;
        public RangedAnimationEventBridge Bridge => _bridge;

        public bool IsValid => null != _data && null != _bridge && null != _data.ProjectilePrefab;
    }
}
