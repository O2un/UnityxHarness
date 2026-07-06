using System;
using O2un.Manager;
using UnityEngine;

public class EnemyContext : MonoBehaviour, IPoolable
{
    Action _release;
    public void SetReleaseCallback(Action release) => _release = release;

    public void Release()
    {
        _release?.Invoke();
    }
}
