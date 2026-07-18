using System;
using R3;

namespace O2un.ProjectB.Platformer
{
    public sealed class AnimatedAttackHook : IAttackHook, IDisposable
    {
        private readonly IEnemyAttackExecutor _executor;
        private readonly AttackCooldownModule _cooldown;
        private readonly IDisposable _subscription;

        public AnimatedAttackHook(IEnemyAttackExecutor executor, float cooldown)
        {
            _executor = executor;
            _cooldown = new AttackCooldownModule(cooldown);
            _subscription = _executor?.OnAttackAnimationEnd.Subscribe(_ => _cooldown.NotifyAnimationEnded());
        }

        public bool IsFinished => _cooldown.IsFinished;

        public void Begin()
        {
            _cooldown.Begin();

            if (null == _executor)
            {
                _cooldown.NotifyAnimationEnded();
                return;
            }

            _executor.PlayAttack();
        }

        public void Tick(float dt)
        {
            _cooldown.Tick(dt);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
