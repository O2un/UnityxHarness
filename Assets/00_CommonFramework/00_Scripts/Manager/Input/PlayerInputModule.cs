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
        private readonly Subject<Unit> _jumpReleased = new();
        private readonly Subject<Unit> _attack = new();

        public ReadOnlyReactiveProperty<Vector2> Move => _move;
        public Observable<Unit> Jump => _jump;
        public Observable<Unit> JumpReleased => _jumpReleased;
        public Observable<Unit> Attack => _attack;

        public void Dispose()
        {
            _attack.Dispose();
            _jumpReleased.Dispose();
            _jump.Dispose();
            _move.Dispose();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                _attack.OnNext(Unit.Default);
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                _jump.OnNext(Unit.Default);
            }
            else if(context.canceled)
            {
                _jumpReleased.OnNext(Unit.Default);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _move.Value = context.ReadValue<Vector2>();
        }

        public void OnSkiiS(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnSkillA(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }
    }
}
