using System;
using UnityEngine;

public class MonoHook : MonoBehaviour
{
    public event Action destroyed;

    private void OnDestroy()
    {
        destroyed?.Invoke();
    }
}