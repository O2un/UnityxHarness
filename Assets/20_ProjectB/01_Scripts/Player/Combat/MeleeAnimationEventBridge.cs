using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class MeleeAnimationEventBridge : MonoBehaviour
    {
        private readonly Subject<Unit> _onHitboxOn = new();
        private readonly Subject<Unit> _onHitboxOff = new();
        private readonly Subject<Unit> _onComboWindowOpen = new();
        private readonly Subject<Unit> _onComboWindowClose = new();
        private readonly Subject<Unit> _onAttackEnd = new();

        public Observable<Unit> OnHitboxOn => _onHitboxOn;
        public Observable<Unit> OnHitboxOff => _onHitboxOff;
        public Observable<Unit> OnComboWindowOpen => _onComboWindowOpen;
        public Observable<Unit> OnComboWindowClose => _onComboWindowClose;
        public Observable<Unit> OnAttackEnd => _onAttackEnd;

        public void AnimEvent_HitboxOn()
        {
            _onHitboxOn.OnNext(Unit.Default);
        }

        public void AnimEvent_HitboxOff()
        {
            _onHitboxOff.OnNext(Unit.Default);
        }

        public void AnimEvent_ComboWindowOpen()
        {
            _onComboWindowOpen.OnNext(Unit.Default);
        }

        public void AnimEvent_ComboWindowClose()
        {
            _onComboWindowClose.OnNext(Unit.Default);
        }

        public void AnimEvent_AttackEnd()
        {
            _onAttackEnd.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            _onHitboxOn.Dispose();
            _onHitboxOff.Dispose();
            _onComboWindowOpen.Dispose();
            _onComboWindowClose.Dispose();
            _onAttackEnd.Dispose();
        }
    }
}
