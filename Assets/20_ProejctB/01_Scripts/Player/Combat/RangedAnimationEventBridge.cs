using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class RangedAnimationEventBridge : MonoBehaviour
    {
        private readonly Subject<Unit> _onFireProjectile = new();
        private readonly Subject<Unit> _onSkillEnd = new();

        public Observable<Unit> OnFireProjectile => _onFireProjectile;
        public Observable<Unit> OnSkillEnd => _onSkillEnd;

        public void AnimEvent_FireProjectile()
        {
            _onFireProjectile.OnNext(Unit.Default);
        }

        public void AnimEvent_SkillEnd()
        {
            _onSkillEnd.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _onFireProjectile.Dispose();
            _onSkillEnd.Dispose();
        }
    }
}
