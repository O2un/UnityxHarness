using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class SelfTargetStrategy : ITargetStrategy
    {
        public IActor Select(IActorQuery query, Vector3 origin)
        {
            // WHY: 오라·장판은 오너 중심으로 발동하므로 외부 타깃을 선정하지 않는다
            return null;
        }
    }
}
