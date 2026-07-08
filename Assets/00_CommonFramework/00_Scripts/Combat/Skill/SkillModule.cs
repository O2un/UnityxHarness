using System;
using System.Collections.Generic;

namespace O2un.Combat
{
    public sealed class SkillModule : IDisposable
    {
        private readonly ISkillContext _ctx;
        private readonly ISkillDefinition[] _defs;
        private readonly float[] _timers;

        public SkillModule(IReadOnlyList<ISkillDefinition> defs, ISkillContext ctx)
        {
            _ctx = ctx;
            _defs = new ISkillDefinition[defs.Count];
            for (int i = 0; i < defs.Count; i++)
            {
                _defs[i] = defs[i];
            }

            _timers = new float[_defs.Length];
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < _defs.Length; i++)
            {
                ISkillDefinition def = _defs[i];
                _timers[i] += dt;

                if (_timers[i] < def.Cooldown)
                {
                    continue;
                }

                _timers[i] = 0f;
                def.Activate(_ctx);
            }
        }

        public void Dispose()
        {
            // NULL
        }
    }
}
