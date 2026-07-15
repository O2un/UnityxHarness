using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class PlayerMover
    {
        private readonly float _maxMoveSpeed;
        private readonly float _jumpVelocity;

        private float _moveX;
        private bool _jumpQueued;
        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;

        public PlayerMover(MovementData data)
        {
            _maxMoveSpeed = data.MaxMoveSpeed;
            _jumpVelocity = data.JumpVelocity;
        }

        public void SetMoveInput(float moveX)
        {
            _moveX = moveX;
        }

        public void QueueJump()
        {
            _jumpQueued = true;
        }

        public Vector2 ResolveVelocity(bool grounded, float currentVerticalVelocity)
        {
            _isGrounded = grounded;

            float x = _moveX * _maxMoveSpeed;
            float y = currentVerticalVelocity;

            if (true == _jumpQueued && true == grounded)
            {
                y = _jumpVelocity;
                _jumpQueued = false;
            }

            return new Vector2(x, y);
        }
    }
}
