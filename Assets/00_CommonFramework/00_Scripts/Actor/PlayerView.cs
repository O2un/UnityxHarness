using O2un.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace O2un.Actors 
{
    public class PlayerView : MonoBehaviour
    {
        private Vector3 _moveDir;
        public void SetVelocity(Vector3 v)
        {
            _moveDir = v;
        }


        private void FixedUpdate()
        {
            transform.Translate(_moveDir * Time.fixedDeltaTime);
        }
    }
}
