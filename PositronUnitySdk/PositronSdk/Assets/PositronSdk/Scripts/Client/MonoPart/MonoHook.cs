using System;
using UnityEngine;

namespace Positron.Client.Mono
{
    public class MonoHook : MonoBehaviour
    {
        public event Action destroyed;

        private void OnDestroy()
        {
            destroyed?.Invoke();
        }
    }
}