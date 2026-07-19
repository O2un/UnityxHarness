using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public readonly struct MeleeHitResult
    {
        public int Damage { get; }
        public bool IsCritical { get; }

        public MeleeHitResult(int damage, bool isCritical)
        {
            Damage = damage;
            IsCritical = isCritical;
        }
    }

    public sealed class PassiveSkillModule
    {
        private readonly PassiveSkillData _data;
        private readonly IPassiveSkillQuery _query;

        private float _missileCooldownTimer;

        public bool IsHomingMissileUnlocked => _query.IsUnlocked(PassiveSkillType.HomingMissile);

        public bool CanFireMissile => true == IsHomingMissileUnlocked && _missileCooldownTimer <= 0f;

        public PassiveSkillModule(PassiveSkillData data, IPassiveSkillQuery query)
        {
            _data = data;
            _query = query;
        }

        public void Tick(float deltaTime)
        {
            if (_missileCooldownTimer > 0f)
            {
                _missileCooldownTimer -= deltaTime;
            }
        }

        public MeleeHitResult ResolveMeleeDamage(int damage)
        {
            // 미해금이면 난수도 소비하지 않는다. 획득 전 전투가 기존과 완전히 같아야 한다.
            if (false == _query.IsUnlocked(PassiveSkillType.CriticalOnHit))
            {
                return new MeleeHitResult(Mathf.Max(1, damage), false);
            }

            if (Random.value >= _data.CriticalChance)
            {
                return new MeleeHitResult(Mathf.Max(1, damage), false);
            }

            int critical = Mathf.FloorToInt(damage * _data.CriticalMultiplier);
            return new MeleeHitResult(Mathf.Max(1, critical), true);
        }

        public void NotifyMissileFired()
        {
            _missileCooldownTimer = _data.MissileCooldown;
        }
    }
}
