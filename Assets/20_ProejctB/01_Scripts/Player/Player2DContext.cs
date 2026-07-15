using O2un.DI;
using O2un.Input;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private MovementData _data;
        [SerializeField] private PlayerView _view;

        [Inject] private IInputReader _input;

        private readonly CompositeDisposable _disposables = new();

        private PlayerMover _mover;
        private float _moveX;

        public void Init()
        {
            if (null == _input || null == _data || null == _view)
            {
                Debug.LogError($"[Player2DContext] '{name}' 의존성 주입 실패 — input={_input != null}, data={_data != null}, view={_view != null}");
                return;
            }

            _mover = new PlayerMover(_data);

            _input.Move.Subscribe(v => _moveX = v.x).AddTo(_disposables);
            _input.IsJumpPressed.Subscribe(_ => _mover.QueueJump()).AddTo(_disposables);
        }

        private void Update()
        {
            _mover?.SetMoveInput(_moveX);
        }

        private void FixedUpdate()
        {
            if (null == _mover)
            {
                return;
            }

            bool grounded = _view.CheckGrounded(_data.GroundMask, _data.GroundCastSize, _data.GroundCastDistance);
            Vector2 velocity = _mover.ResolveVelocity(grounded, _view.VerticalVelocity);
            _view.ApplyVelocity(velocity);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
