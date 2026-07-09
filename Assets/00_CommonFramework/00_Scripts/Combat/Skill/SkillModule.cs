using System;
using System.Collections.Generic;

namespace O2un.Combat
{
    public sealed class SkillModule : IDisposable
    {
        private sealed class SkillSlot
        {
            public ISkillDefinition Definition;
            public float Timer;
        }

        private readonly ISkillContext _ctx;
        private readonly List<SkillSlot> _slots;

        public SkillModule(IReadOnlyList<ISkillDefinition> defs, ISkillContext ctx)
        {
            _ctx = ctx;
            _slots = new List<SkillSlot>(defs.Count);
            for (int i = 0; i < defs.Count; i++)
            {
                _slots.Add(new SkillSlot { Definition = defs[i], Timer = 0f });
            }
        }

        public bool AcquireOrUpgrade(ISkillDefinition definition)
        {
            if (true == string.IsNullOrEmpty(definition.SkillId))
            {
                _slots.Add(new SkillSlot { Definition = definition, Timer = 0f });
                return true;
            }

            SkillSlot existing = FindSlot(definition.SkillId);
            if (null == existing)
            {
                _slots.Add(new SkillSlot { Definition = definition, Timer = 0f });
                return true;
            }

            if (definition.Level > existing.Definition.Level)
            {
                existing.Definition = definition;
                return true;
            }

            return false;
        }

        public bool ApplyUpgrade(string skillId, SkillUpgradeData upgrade)
        {
            SkillSlot existing = FindSlot(skillId);
            if (null == existing)
            {
                return false;
            }

            return existing.Definition.ApplyUpgrade(upgrade);
        }

        public int GetSkillLevel(string skillId)
        {
            SkillSlot existing = FindSlot(skillId);
            return null != existing ? existing.Definition.Level : 0;
        }

        public bool HasSkill(string skillId)
        {
            return null != FindSlot(skillId);
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                SkillSlot slot = _slots[i];
                slot.Timer += dt;

                if (slot.Timer < slot.Definition.Cooldown)
                {
                    continue;
                }

                slot.Timer = 0f;
                slot.Definition.Activate(_ctx);
            }
        }

        private SkillSlot FindSlot(string skillId)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (skillId == _slots[i].Definition.SkillId)
                {
                    return _slots[i];
                }
            }

            return null;
        }

        public void Dispose()
        {
            // NULL
        }
    }
}
