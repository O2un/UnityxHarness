using O2un.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace O2un.Actors 
{
    public class PlayerView : MonoBehaviour
    {
        [Inject] private IInputReader _input;
        [SerializeField] private float _speed;

        void Update()
        {
            var dir = new Vector3(_input.Move.x , 0 , _input.Move.y);
            transform.Translate(dir * _speed * Time.deltaTime);
        }
    }
}
