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
        private readonly Subject<Unit> _skill = new();
        private readonly Subject<Unit> _inventory = new();

        public ReadOnlyReactiveProperty<Vector2> Move => _move;
        public Observable<Unit> Jump => _jump;
        public Observable<Unit> JumpReleased => _jumpReleased;
        public Observable<Unit> Attack => _attack;
        public Observable<Unit> Skill => _skill;
        public Observable<Unit> Inventory => _inventory;

        public void Dispose()
        {
            _inventory.Dispose();
            _skill.Dispose();
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

        public void OnInventory(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                _inventory.OnNext(Unit.Default);
            }
        }

        public void OnSkiiS(InputAction.CallbackContext context)
        {
            // NULL
        }

        public void OnSkillA(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                _skill.OnNext(Unit.Default);
            }
        }
    }
}
