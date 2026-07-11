using UnityEngine;

namespace O2un.Actors
{
    public sealed class EnemyBlackboard
    {
        public Transform Transform { get; set; }
        public Vector3 SelfPosition { get; set; }
        public Vector3 TargetPosition { get; set; }
        public bool HasTarget { get; set; }
    }
}
