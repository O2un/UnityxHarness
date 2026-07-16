using System;
using R3;

namespace O2un.ProjectB.Platformer
{
    public sealed class RangedSkillModule : IDisposable
    {
        private readonly float _cooldown;
        private readonly float _maxCastTime;

        private float _cooldownTimer;
        private float _castTimer;
        private bool _isCasting;

        public bool IsCasting => _isCasting;
        public bool IsOnCooldown => 0f < _cooldownTimer;

        private readonly Subject<Unit> _onActivated = new();
        public Observable<Unit> OnActivated => _onActivated;

        public RangedSkillModule(float cooldown, float maxCastTime)
        {
            _cooldown = cooldown;
            _maxCastTime = maxCastTime;
        }

        public bool TryActivate()
        {
            if (true == IsOnCooldown)
            {
                return false;
            }

            if (true == _isCasting)
            {
                return false;
            }

            _cooldownTimer = _cooldown;
            _castTimer = _maxCastTime;
            _isCasting = true;
            _onActivated.OnNext(Unit.Default);
            return true;
        }

        public void NotifyCastEnd()
        {
            _isCasting = false;
            _castTimer = 0f;
        }

        public void Tick(float dt)
        {
            if (0f < _cooldownTimer)
            {
                _cooldownTimer -= dt;
                if (0f >= _cooldownTimer)
                {
                    _cooldownTimer = 0f;
                }
            }

            if (true == _isCasting && 0f < _castTimer)
            {
                _castTimer -= dt;
                // 전이 블렌딩·피격 등으로 SkillEnd 이벤트가 스킵되어도 영구 잠금되지 않도록 상한에서 강제 해제
                if (0f >= _castTimer)
                {
                    NotifyCastEnd();
                }
            }
        }

        public void Dispose()
        {
            _onActivated.Dispose();
        }
    }
}
