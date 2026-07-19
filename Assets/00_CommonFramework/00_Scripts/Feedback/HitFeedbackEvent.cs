using O2un.Actors;
using UnityEngine;

namespace O2un.Feedback
{
    public readonly struct HitFeedbackEvent
    {
        public HitFeedbackEvent(ActorType team, int damage, Vector3 position)
        {
            Team = team;
            Damage = damage;
            Position = position;
        }

        public ActorType Team { get; }
        public int Damage { get; }
        public Vector3 Position { get; }
    }
}
