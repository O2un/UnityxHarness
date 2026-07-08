using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class SkillContext : ISkillContext
    {
        private readonly IActorQuery _query;
        private readonly IAttackSpawner _spawner;
        private readonly Transform _owner;

        public SkillContext(IActorQuery query, IAttackSpawner spawner, Transform owner)
        {
            _query = query;
            _spawner = spawner;
            _owner = owner;
        }

        public IActorQuery Query => _query;
        public IAttackSpawner Spawner => _spawner;
        public Transform Owner => _owner;
        public Vector3 OwnerPosition => _owner.position;
        public Quaternion OwnerRotation => _owner.rotation;
    }
}
