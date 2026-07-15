using O2un.Actors;
using O2un.DI;
using O2un.Input;
using UnityEngine;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private MovementData _data;
        [SerializeField] private PlayerView _view;

        [Inject] private IInputReader _input;
        [Inject] private IActorRegistry _registry;

        private Player2DActor _actor;

        public void Init()
        {
            if (null == _input || null == _data || null == _view || null == _registry)
            {
                Debug.LogError($"[Player2DContext] '{name}' 의존성 주입 실패 — input={_input != null}, data={_data != null}, view={_view != null}, registry={_registry != null}");
                return;
            }

            _actor = new Player2DActor(_data, _input, _view, _registry);
        }

        private void Update()
        {
            _actor?.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _actor?.FixedTick(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            _actor?.Dispose();
        }
    }
}
