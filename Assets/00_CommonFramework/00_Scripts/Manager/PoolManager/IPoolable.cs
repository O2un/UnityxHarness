using System;

namespace O2un.Manager
{
    public interface IPoolable
    {
        void SetReleaseCallback(Action release);
        void OnSpawned();
        void OnDespawned();
    }
}
