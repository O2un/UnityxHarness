using System;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [Serializable]
    public sealed class MeleeComboStage
    {
        [SerializeField, Min(0)] private int _damage;
        [SerializeField] private Vector2 _hitboxSize;
        [SerializeField] private Vector2 _hitboxOffset;

        public int Damage => _damage;
        public Vector2 HitboxSize => _hitboxSize;
        public Vector2 HitboxOffset => _hitboxOffset;
    }

    [CreateAssetMenu(menuName = "ProjectB/Platformer/MeleeComboData")]
    public sealed class MeleeComboData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _inputBufferTime;
        [SerializeField] private MeleeComboStage[] _stages;

        public float InputBufferTime => _inputBufferTime;
        public int StageCount => _stages?.Length ?? 0;
        public MeleeComboStage GetStage(int stage) => _stages[stage - 1];
    }
}
