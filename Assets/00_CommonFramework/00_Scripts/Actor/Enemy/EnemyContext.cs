using System;
using O2un.Manager;
using UnityEngine;

public class EnemyContext : MonoBehaviour, IPoolable
{
    Action _release;
    Action _onSpawned;
    Action _onDespawned;

    public void SetReleaseCallback(Action release) => _release = release;

    public void SetLifecycleCallbacks(Action onSpawned, Action onDespawned)
    {
        _onSpawned = onSpawned;
        _onDespawned = onDespawned;
    }

    public void Release()
    {
        _release?.Invoke();
    }

    public void OnSpawned()
    {
        _onSpawned?.Invoke();
    }

    public void OnDespawned()
    {
        _onDespawned?.Invoke();
    }
}
