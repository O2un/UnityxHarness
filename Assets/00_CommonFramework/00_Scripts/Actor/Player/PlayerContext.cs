using O2un.DataStore;
using UnityEngine;
using VContainer;

namespace O2un.Actors
{
    public sealed class PlayerContext : MonoBehaviour
    {
        [SerializeField] private MoveStats _stats;
        [SerializeField] private ActorView _view;
        private PlayerActor _actor;

        [Inject]
        public void Init(IMoveDirectionProvider provider, IPlayerDataWriter playerData, IActorRegistry registry)
        {
            _actor = new(provider, _view, playerData, _stats, registry);
            _actor.Init();
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
