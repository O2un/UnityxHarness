using System;
using R3;

namespace O2un.UI
{
    public sealed class GameSelectVM : IDisposable
    {
        private readonly Subject<Unit> _projectASelected = new();
        private readonly Subject<Unit> _projectBSelected = new();

        public Observable<Unit> ProjectASelected => _projectASelected;
        public Observable<Unit> ProjectBSelected => _projectBSelected;

        public void OnProjectAClicked()
        {
            _projectASelected.OnNext(Unit.Default);
        }

        public void OnProjectBClicked()
        {
            _projectBSelected.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _projectASelected.Dispose();
            _projectBSelected.Dispose();
        }
    }
}
