using System;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace O2un.Input 
{
    public enum InputType
    {
        Player,
        UI,    
    }

    public sealed class InputManager : IInputReader, IInitializable, IDisposable
    {
        public ReadOnlyReactiveProperty<Vector2> Move => _playerInput.Move;
        public Observable<Unit> IsJumpPressed => _playerInput.Jump;
        public Observable<Unit> IsJumpReleased => _playerInput.JumpReleased;
        public Observable<Unit> IsAttackPressed => _playerInput.Attack;
        public Observable<Unit> IsSkillPressed => _playerInput.Skill;
        public Observable<Unit> IsInventoryPressed => _playerInput.Inventory;
        private readonly GameInput _inputActions = new();
        private readonly PlayerInputModule _playerInput = new();
        private readonly UIInputModule _uiInput  = new();
        public void Initialize()
        {
            _inputActions.Player.SetCallbacks(_playerInput);
            _inputActions.UI.SetCallbacks(_uiInput);

            _inputActions.Player.Enable();
        }

        public void SwitchInput(InputType type)
        {
            switch (type)
            {
                case InputType.Player:
                    {
                        _inputActions.Player.Enable();
                        _inputActions.UI.Disable();
                    }
                    break;
                case InputType.UI:
                    {
                        _inputActions.UI.Enable();
                        _inputActions.Player.Disable();
                    }
                    break;
            }
        }

        public void Dispose()
        {
            _inputActions.Player.SetCallbacks(null);
            _inputActions.UI.SetCallbacks(null);

            _inputActions.Disable();
            _inputActions.Dispose();
        }
    }
}
