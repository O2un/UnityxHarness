using System;

namespace O2un.UI
{
    public sealed class GameSelectVM
    {
        public bool IsProjectBEnabled => false;

        public event Action ProjectASelected;

        public void OnProjectAClicked()
        {
            ProjectASelected?.Invoke();
        }
    }
}
