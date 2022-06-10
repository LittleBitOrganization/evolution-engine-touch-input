using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    internal class ClickService : IClickService
    {
        private bool _isClickPrevented = false;
        
        public event InputClickDelegate OnInputClick;
        public event InputPositionDelegate OnInputClickUp;
        public event InputPositionDelegate OnInputClickDown;

        private float _lastFingerDownTimeReal = 0;
        private float _lastClickTimeReal = 0;
        
        private bool _wasFingerDownLastFrame = false;
        private bool _isFingerDown = false;
        
        private Vector3 _lastFinger0DownPos = Vector3.zero;
        private Vector3 _lastFinger1DownPos;
        public bool IsClickPrevented
        {
            get => _isClickPrevented;
            set => _isClickPrevented = value;
        }

        public bool WasFingerDownLastFrame
        {
            get => _wasFingerDownLastFrame;
            set => _wasFingerDownLastFrame = value;
        }

        public float LastClickTimeReal
        {
            get => _lastClickTimeReal;
            set => _lastClickTimeReal = value;
        }
        public float LastFingerDownTimeReal
        {
            get => _lastFingerDownTimeReal;
            set => _lastFingerDownTimeReal = value;
        }

        public Vector3 LastFinger0DownPos
        {
            get => _lastFinger0DownPos;
            set => _lastFinger0DownPos = value;
        }

        public void FingerDown()
        {
            _isFingerDown = true;
            OnInputClickDown?.Invoke(TouchWrapper.AverageTouchPos);
        }

        public void Update()
        {
            if (TouchWrapper.TouchCount == 0)
            {
                _isClickPrevented = false;
                if (_isFingerDown == true)
                {
                    _isFingerDown = false;
                    OnInputClickUp?.Invoke(TouchWrapper.AverageTouchPos);
                }
            }
        }
    }
}