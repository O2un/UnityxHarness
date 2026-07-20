using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public interface IEnemy2DBlackboard
    {
        Vector2 SelfPosition { get; }
        float Facing { get; }

        bool HasTarget { get; }
        Vector2 TargetPosition { get; }

        bool IsPlayerVisible { get; }
        bool HeardSoundThisTick { get; }
        bool WasHitThisTick { get; }

        bool GroundAhead { get; }
        bool WallAhead { get; }

        Vector2 LastKnownTargetPosition { get; }
        bool HasLastKnownTargetPosition { get; }
        float TimeSincePerceived { get; }
    }
}
