using O2un.Input;
using UnityEngine;
using VContainer;

namespace O2un.Actors 
{
    public sealed class PlayerContext : MonoBehaviour
    {
        [SerializeField] private PlayerView _view;
        private PlayerActor _actor;

        [Inject]
        public void Init(IInputReader input)
        {
            _actor = new(input, _view);
            _actor.Init();
        }

        private void Oestroy()
        {
            _actor?.Dispose();
        }
    }
}
