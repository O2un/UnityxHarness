using System.Collections.Generic;
using O2un.Combat;
using O2un.DataStore;
using O2un.DI;
using UnityEngine;
using VContainer;

namespace O2un.Actors
{
    public sealed class PlayerContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private MoveStats _stats;
        [SerializeField] private ActorView _view;
        [SerializeField] private SkillDefinitionSO[] _skills;
        [SerializeField] private DamageableView _damageable;

        [Inject] private IMoveDirectionProvider _provider;
        [Inject] private IPlayerDataWriter _playerData;
        [Inject] private PlayerHealthAdapter _health;
        [Inject] private IActorRegistry _registry;
        [Inject] private IActorQuery _query;
        [Inject] private IAttackSpawner _spawner;

        private PlayerActor _actor;

        public void Init()
        {
            if (null == _provider || null == _playerData || null == _health || null == _registry || null == _query || null == _spawner)
            {
                Debug.LogError($"[PlayerContext] '{name}' 의존성 주입 실패 — provider={_provider != null}, playerData={_playerData != null}, health={_health != null}, registry={_registry != null}, query={_query != null}, spawner={_spawner != null}");
                return;
            }

            SkillModule skillModule = BuildSkills(_query, _spawner);

            _actor = new(_provider, _view, _playerData, _stats, _registry, skillModule);
            _actor.Init();

            if (null != _damageable)
            {
                _damageable.Bind(ActorType.Player, _health);
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
