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

        private Player2DActor _actor;

        public void Init()
        {
            if (null == _input || null == _data || null == _view)
            {
                Debug.LogError($"[Player2DContext] '{name}' 의존성 주입 실패 — input={_input != null}, data={_data != null}, view={_view != null}");
                return;
            }

            _actor = new Player2DActor(_data, _input);
        }

        private void Update()
        {
            _actor?.Tick();
        }

        private void FixedUpdate()
        {
            if (null == _actor)
            {
                return;
            }

            bool grounded = _view.CheckGrounded(_data.GroundMask, _data.GroundCastSize, _data.GroundCastDistance);
            Vector2 velocity = _actor.ResolvePhysics(grounded, _view.VerticalVelocity);
            _view.ApplyVelocity(velocity);
        }

        private void OnDestroy()
        {
            _actor?.Dispose();
        }
    }
}
