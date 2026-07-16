using System;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class MeleeAnimationEventBridge : MonoBehaviour
    {
        public event Action OnHitboxOn;
        public event Action OnHitboxOff;
        public event Action OnComboWindowOpen;
        public event Action OnComboWindowClose;
        public event Action OnAttackEnd;

        public void AnimEvent_HitboxOn()
        {
            OnHitboxOn?.Invoke();
        }

        public void AnimEvent_HitboxOff()
        {
            OnHitboxOff?.Invoke();
        }

        public void AnimEvent_ComboWindowOpen()
        {
            OnComboWindowOpen?.Invoke();
        }

        public void AnimEvent_ComboWindowClose()
        {
            OnComboWindowClose?.Invoke();
        }

        public void AnimEvent_AttackEnd()
        {
            OnAttackEnd?.Invoke();
        }
    }
}
