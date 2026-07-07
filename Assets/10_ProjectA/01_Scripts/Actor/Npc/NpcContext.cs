using UnityEngine;
using VContainer;

namespace O2un.Actors
{
    public sealed class NpcContext : MonoBehaviour
    {
        [SerializeField] private MoveStats _stats;
        [SerializeField] private ActorView _view;
        [SerializeField] private ChaseAIProfile _profile;

        private NpcActor _actor;
        private IActorRegistry _registry;
        private IActorQuery _query;

        [Inject]
        public void Construct(IActorRegistry registry, IActorQuery query)
        {
            _registry = registry;
            _query = query;
            Build();
        }

        private void Build()
        {
            EnemyBlackboard blackboard = new();
            CharacterMover mover = new(_stats);
            _actor = new NpcActor(_profile.Build(blackboard, mover), blackboard, mover, _view, _registry, _query);
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
