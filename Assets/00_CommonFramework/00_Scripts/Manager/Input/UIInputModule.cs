using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace O2un.Input
{
    // 포인터·네비게이션은 EventSystem의 InputSystemUIInputModule이 액션에서 직접 읽어 처리한다.
    // 아래 콜백은 GameInput.IUIActions 계약을 채우기 위한 것으로, 게임 코드가 UI 입력을 직접 구독해야 할 때만 채운다
    public sealed class UIInputModule : GameInput.IUIActions, IDisposable
    {
        public void Dispose()
        {

        }

        public void OnNavigate(InputAction.CallbackContext context)
        {

        }

        public void OnSubmit(InputAction.CallbackContext context)
        {

        }

        public void OnCancel(InputAction.CallbackContext context)
        {

        }

        public void OnPoint(InputAction.CallbackContext context)
        {

        }

        public void OnClick(InputAction.CallbackContext context)
        {

        }

        public void OnRightClick(InputAction.CallbackContext context)
        {

        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {

        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {

        }
    }
}
