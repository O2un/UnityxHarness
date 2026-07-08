using System.Collections.Generic;
using O2un.Combat;
using O2un.DataStore;
using UnityEngine;
using VContainer;

namespace O2un.Actors
{
    public sealed class PlayerContext : MonoBehaviour
    {
        [SerializeField] private MoveStats _stats;
        [SerializeField] private ActorView _view;
        [SerializeField] private SkillDefinitionSO[] _skills;
        [SerializeField] private DamageableView _damageable;

        private PlayerActor _actor;

        [Inject]
        public void Init(
            IMoveDirectionProvider provider,
            IPlayerDataWriter playerData,
            IPlayerDataReader playerReader,
            IActorRegistry registry,
            IActorQuery query,
            IAttackSpawner spawner)
        {
            SkillModule skillModule = BuildSkills(query, spawner);

            _actor = new(provider, _view, playerData, _stats, registry, skillModule);
            _actor.Init();

            if (null != _damageable)
            {
                IHealth health = new PlayerHealthAdapter(playerReader, playerData);
                _damageable.Bind(ActorType.Player, health);
            }
        }

        private SkillModule BuildSkills(IActorQuery query, IAttackSpawner spawner)
        {
            List<ISkillDefinition> defs = new(_skills.Length);
            for (int i = 0; i < _skills.Length; i++)
            {
                if (null == _skills[i])
                {
                    continue;
                }

                defs.Add(_skills[i].Build());
            }

            SkillContext context = new(query, spawner, _view.transform);
            return new SkillModule(defs, context);
        }

        private void Update()
        {
            _actor?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _actor?.Dispose();
        }
    }
}
