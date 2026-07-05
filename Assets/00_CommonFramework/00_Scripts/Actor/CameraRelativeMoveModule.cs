using O2un.Camera;
using O2un.Input;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class CameraRelativeMoveModule : IMoveDirectionProvider
    {
        private readonly IInputReader _input;
        private readonly ICameraBasisProvider _camera;

        public CameraRelativeMoveModule(IInputReader input, ICameraBasisProvider camera)
        {
            _input = input;
            _camera = camera;
        }

        public Vector3 GetDirection()
        {
            Vector2 move = _input.Move.CurrentValue;
            Vector3 dir = _camera.PlanarForward * move.y + _camera.PlanarRight * move.x;

            if (dir.sqrMagnitude > 1f)
            {
                dir.Normalize();
            }

            return dir;
        }
    }
}
