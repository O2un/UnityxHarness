using R3;
using UnityEngine;
using UnityEngine.UI;

namespace O2un.UI
{
    public sealed class GameSelectView : MonoBehaviour
    {
        [SerializeField] private Button _projectAButton;
        [SerializeField] private Button _projectBButton;

        public void Bind(GameSelectVM vm)
        {
            if (null != _projectAButton)
            {
                _projectAButton.OnClickAsObservable().Subscribe(_ => vm.OnProjectAClicked()).AddTo(this);
            }

            if (null != _projectBButton)
            {
                _projectBButton.interactable = true;
                _projectBButton.OnClickAsObservable().Subscribe(_ => vm.OnProjectBClicked()).AddTo(this);
            }
        }
    }
}
