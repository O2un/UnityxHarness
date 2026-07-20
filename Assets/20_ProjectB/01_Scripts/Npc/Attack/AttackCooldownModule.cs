namespace O2un.ProjectB.Platformer
{
    public sealed class AttackCooldownModule
    {
        private readonly float _cooldown;

        private bool _animationEnded;
        private float _elapsedSinceAnimationEnd;

        public AttackCooldownModule(float cooldown)
        {
            _cooldown = cooldown;
        }

        public bool IsFinished => true == _animationEnded && _elapsedSinceAnimationEnd >= _cooldown;

        public void Begin()
        {
            _animationEnded = false;
            _elapsedSinceAnimationEnd = 0f;
        }

        public void NotifyAnimationEnded()
        {
            if (true == _animationEnded)
            {
                return;
            }

            _animationEnded = true;
            _elapsedSinceAnimationEnd = 0f;
        }

        public void Tick(float dt)
        {
            if (false == _animationEnded)
            {
                return;
            }

            _elapsedSinceAnimationEnd += dt;
        }
    }
}
