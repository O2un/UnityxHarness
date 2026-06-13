using O2un.Input;
using UnityEngine;

namespace O2un.Actors 
{
    public class PlayerMover
    {
        private readonly IInputReader _input;
        public PlayerMover(IInputReader input)
        {
            _input = input;
        }

        public void Tick(float dt)
        {
            var dir = new Vector3(_input.Move.x, 0, _input.Move.y);
            // 이동처리
        }
    }
}
