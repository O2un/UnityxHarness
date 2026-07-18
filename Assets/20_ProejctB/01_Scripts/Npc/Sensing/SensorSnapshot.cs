using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public readonly struct SensorSnapshot
    {
        public readonly Vector2 SelfPosition;
        public readonly float Facing;
        public readonly bool HasTarget;
        public readonly Vector2 TargetPosition;
        public readonly bool IsPlayerVisible;
        public readonly bool HeardSound;
        public readonly bool WasHit;
        public readonly bool GroundAhead;
        public readonly bool WallAhead;

        public SensorSnapshot(
            Vector2 selfPosition,
            float facing,
            bool hasTarget,
            Vector2 targetPosition,
            bool isPlayerVisible,
            bool heardSound,
            bool wasHit,
            bool groundAhead,
            bool wallAhead)
        {
            SelfPosition = selfPosition;
            Facing = facing;
            HasTarget = hasTarget;
            TargetPosition = targetPosition;
            IsPlayerVisible = isPlayerVisible;
            HeardSound = heardSound;
            WasHit = wasHit;
            GroundAhead = groundAhead;
            WallAhead = wallAhead;
        }
    }
}
