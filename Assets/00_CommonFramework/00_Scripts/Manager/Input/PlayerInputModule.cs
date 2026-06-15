using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace O2un.Input 
{
    public sealed class PlayerInputModule : GameInput.IPlayerActions, IDisposable
    {
        private readonly ReactiveProperty<Vector2> _move = new();
        private readonly Subject<Unit> _jump = new();

        public ReadOnlyReactiveProperty<Vector2> Move => _move;
        public Observable<Unit> Jump => _jump;

        public void Dispose()
        {
            _jump.Dispose();
            _move.Dispose();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                _jump.OnNext(Unit.Default);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _move.Value = context.ReadValue<Vector2>();
        }
    }
}
