using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public struct AttackRequest
    {
        public AttackHitboxView Prefab;
        public string PoolKey;

        public Vector3 Origin;
        public Quaternion Rotation;

        public Vector3 MoveDirection;
        public float Speed;
        public Transform FollowOwner;
        public Vector3 FollowOffset;
        public Quaternion FollowRotation;

        public float Lifetime;
        public float ReHitInterval;
        public bool ReleaseOnHit;

        public HitPolicy Policy;
        public int Damage;
        public ActorType TargetTeam;
    }
}
