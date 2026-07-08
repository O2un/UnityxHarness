using System.Collections.Generic;
using R3;

namespace O2un.Combat
{
    public sealed class HitboxModule
    {
        private readonly HitboxConfig _config;
        private readonly Dictionary<IDamageable, float> _lastHitTimes = new();
        private readonly Subject<DamageEvent> _onHit = new();

        private float _age;

        public HitboxModule(HitboxConfig config)
        {
            _config = config;
        }

        public Observable<DamageEvent> OnHit => _onHit;
        public bool IsExpired => _age >= _config.Lifetime;

        public void Tick(float dt)
        {
            _age += dt;
        }

        public bool TryHit(IDamageable target)
        {
            if (null == target)
            {
                return false;
            }

            if (target.Team != _config.TargetTeam)
            {
                return false;
            }

            if (false == CanHit(target))
            {
                return false;
            }

            _lastHitTimes[target] = _age;
            _onHit.OnNext(new DamageEvent(target, _config.Damage));
            return true;
        }

        public void Reset()
        {
            _age = 0f;
            _lastHitTimes.Clear();
        }

        private bool CanHit(IDamageable target)
        {
            if (HitPolicy.OncePerTarget == _config.Policy)
            {
                return false == _lastHitTimes.ContainsKey(target);
            }

            if (false == _lastHitTimes.TryGetValue(target, out float lastTime))
            {
                return true;
            }

            return _age - lastTime >= _config.ReHitInterval;
        }
    }
}
