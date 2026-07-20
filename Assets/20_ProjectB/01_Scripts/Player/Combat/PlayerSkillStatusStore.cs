using System;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public interface IPlayerSkillStatusReader
    {
        ReadOnlyReactiveProperty<float> RangedCooldownNormalized { get; }
    }

    public interface IPlayerSkillStatusWriter
    {
        void SetRangedCooldownNormalized(float normalized);
    }

    public sealed class PlayerSkillStatusStore : IPlayerSkillStatusReader, IPlayerSkillStatusWriter, IDisposable
    {
        private readonly ReactiveProperty<float> _rangedCooldown = new();

        public ReadOnlyReactiveProperty<float> RangedCooldownNormalized => _rangedCooldown;

        public void SetRangedCooldownNormalized(float normalized)
        {
            _rangedCooldown.Value = Mathf.Clamp01(normalized);
        }

        public void Dispose()
        {
            _rangedCooldown.Dispose();
        }
    }
}
