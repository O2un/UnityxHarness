using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class PlayerMover
    {
        private readonly float _maxMoveSpeed;
        private readonly float _jumpVelocity;
        private readonly float _coyoteTime;
        private readonly float _jumpBufferTime;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private readonly float _jumpCutFactor;

        private float _moveX;
        private bool _isGrounded;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _currentVelocityX;
        private bool _jumpCutRequested;

        public bool IsGrounded => _isGrounded;

        public PlayerMover(MovementData data)
        {
            _maxMoveSpeed = data.MaxMoveSpeed;
            _jumpVelocity = data.JumpVelocity;
            _coyoteTime = data.CoyoteTime;
            _jumpBufferTime = data.JumpBufferTime;
            _acceleration = data.Acceleration;
            _deceleration = data.Deceleration;
            _jumpCutFactor = data.JumpCutFactor;
        }

        public void SetMoveInput(float moveX)
        {
            _moveX = moveX;
        }

        public void QueueJump()
        {
            _jumpBufferTimer = _jumpBufferTime;
        }

        public void RequestJumpCut()
        {
            _jumpCutRequested = true;
        }

        public Vector2 ResolveVelocity(bool grounded, float currentVerticalVelocity, float dt)
        {
            _isGrounded = grounded;

            if (true == grounded)
            {
                _coyoteTimer = _coyoteTime;
            }
            else
            {
                _coyoteTimer -= dt;
            }

            _jumpBufferTimer -= dt;

            float y = currentVerticalVelocity;

            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                y = _jumpVelocity;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
            }

            float targetX = _moveX * _maxMoveSpeed;
            float rate = Mathf.Abs(targetX) > Mathf.Abs(_currentVelocityX) ? _acceleration : _deceleration;
            _currentVelocityX = Mathf.MoveTowards(_currentVelocityX, targetX, rate * dt);

            if (true == _jumpCutRequested)
            {
                if (y > 0f)
                {
                    y *= _jumpCutFactor;
                }

                _jumpCutRequested = false;
            }

            return new Vector2(_currentVelocityX, y);
        }
    }
}
