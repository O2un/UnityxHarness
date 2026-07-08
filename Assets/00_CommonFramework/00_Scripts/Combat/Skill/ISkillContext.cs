using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public interface ISkillContext
    {
        IActorQuery Query { get; }
        Vector3 OwnerPosition { get; }
        Quaternion OwnerRotation { get; }

        // WHY: 오라·장판 히트박스가 이동하는 오너를 매 프레임 추종하려면 살아있는 Transform 참조가 필요하다
        Transform Owner { get; }

        IAttackSpawner Spawner { get; }
    }
}
