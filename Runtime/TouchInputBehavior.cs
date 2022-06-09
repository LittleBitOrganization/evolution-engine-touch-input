using System;
using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    public class TouchInputBehavior : MonoBehaviour
    {
        public event Action OnUpdate;
        private void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}