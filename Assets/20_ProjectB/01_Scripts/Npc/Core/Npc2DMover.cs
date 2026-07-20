using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Npc2DMover
    {
        private const float DIRECTION_EPSILON = 0.01f;

        private float _facing = 1f;
        private float _velocityX;

        public float Facing => _facing;
        public float VelocityX => _velocityX;

        public void SetFacing(float direction)
        {
            if (Mathf.Abs(direction) < DIRECTION_EPSILON)
            {
                return;
            }

            _facing = 0f < direction ? 1f : -1f;
        }

        public void Flip()
        {
            _facing = -_facing;
        }

        public void MoveForward(float speed)
        {
            _velocityX = _facing * speed;
        }

        public void MoveTowards(float selfX, float targetX, float speed)
        {
            float delta = targetX - selfX;
            if (Mathf.Abs(delta) < DIRECTION_EPSILON)
            {
                Stop();
                return;
            }

            SetFacing(delta);
            MoveForward(speed);
        }

        public void Stop()
        {
            _velocityX = 0f;
        }
    }
}
