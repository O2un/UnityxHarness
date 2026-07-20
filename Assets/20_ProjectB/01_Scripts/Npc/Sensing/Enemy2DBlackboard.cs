using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Enemy2DBlackboard : IEnemy2DBlackboard
    {
        public Vector2 SelfPosition { get; internal set; }
        public float Facing { get; internal set; } = 1f;

        public bool HasTarget { get; internal set; }
        public Vector2 TargetPosition { get; internal set; }

        public bool IsPlayerVisible { get; internal set; }
        public bool HeardSoundThisTick { get; internal set; }
        public bool WasHitThisTick { get; internal set; }

        public bool GroundAhead { get; internal set; } = true;
        public bool WallAhead { get; internal set; }

        public Vector2 LastKnownTargetPosition { get; internal set; }
        public bool HasLastKnownTargetPosition { get; internal set; }
        public float TimeSincePerceived { get; internal set; }
    }
}
