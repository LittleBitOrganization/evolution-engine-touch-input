using System;
using System.Collections.Generic;
using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    internal class DragService : IDragService
    {
        private const float DragDurationThreshold = 0.01f;
        private const int MomentumSamplesCount = 5;
        
        private readonly ITouchInputService _touchInputService;
        private readonly IClickService _clickService;
        private readonly TouchInputConfig _touchInputConfig;
        private readonly IPinchService _pinchService;
        private bool _isDragging = false;
        private Vector3 _dragStartPos = Vector3.zero;
        private Vector3 _dragStartOffset;
        private List<Vector3> _dragFinalMomentumVector;
        private float _timeSinceDragStart = 0;
        private bool _pinchToDragCurrentFrame;
        private bool _wasDraggingLastFrame = false;
        private bool _sendLong = false;

        public event InputDragStartDelegate OnDragStart;
        public event DragUpdateDelegate OnDragUpdate;
        public event DragStopDelegate OnDragStop;
        
        public event Action OnLongTapProgress;
        public DragService(ITouchInputService touchInputService, 
                            IClickService clickService,
                            IPinchService pinchService,
                            TouchInputConfig touchInputConfig)
        {
            _touchInputService = touchInputService;
            _clickService = clickService;
            _touchInputConfig = touchInputConfig;
            _pinchService = pinchService;
            _dragFinalMomentumVector = new List<Vector3>();
        }
        public bool WasDraggingLastFrame
        {
            get => _wasDraggingLastFrame;
            set => _wasDraggingLastFrame = value;
        }
        public bool IsDragging => _isDragging;
        public Vector3 StartDragPosition
        {
            get => _dragStartPos;
            set => _dragStartPos = value;
        }
        public Vector3 DragStartOffset
        {
            get => _dragStartOffset;
            set => _dragStartOffset = value;
        }
        public void Update()
        {
            _pinchToDragCurrentFrame = false;
            if (_pinchService.WasPinchingLastFrame == false)
            {
                if (_clickService.WasFingerDownLastFrame == true && TouchWrapper.IsFingerDown)
                {
                    if (_isDragging == false)
                    {
                        float dragDistance = GetRelativeDragDistance(_touchInputService.GetActualTouchPosition(), _dragStartPos);
                        float dragTime = Time.realtimeSinceStartup - _clickService.LastFingerDownTimeReal;
            
                        bool isLongTap = dragTime > _touchInputConfig.ClickDurationThreshold;
                        
                        float longTapProgress = 0;
                        if (Mathf.Approximately(_touchInputConfig.ClickDurationThreshold, 0) == false)
                        {
                            longTapProgress = Mathf.Clamp01(dragTime / _touchInputConfig.ClickDurationThreshold);
                        }

                        if (!_sendLong&&longTapProgress >= _touchInputConfig.LongTapStartsTime)
                        {
                            _sendLong = true;
                            OnLongTapProgress.Invoke();
                        }
                            
                        
                        if ((dragDistance >= _touchInputConfig.DragStartDistanceThresholdRelative && dragTime >= DragDurationThreshold)
                            || (_touchInputConfig.LongTapStartsDrag == true && isLongTap == true))
                        {
                            _isDragging = true;
                            _dragStartOffset = _clickService.LastFinger0DownPos - _dragStartPos;
                            _dragStartPos = _clickService.LastFinger0DownPos;
                            DragStart(_dragStartPos, isLongTap, true);
                        }
                    }
                }
            }
            else
            {
                if (TouchWrapper.IsFingerDown == true)
                {
                    _isDragging = true;
                    _dragStartPos = _touchInputService.GetActualTouchPosition();
                    DragStart(_dragStartPos, false, false);
                    _pinchToDragCurrentFrame = true;
                }
            }
            
            if (_isDragging == true && TouchWrapper.IsFingerDown == true)
            {
                DragUpdate(_touchInputService.GetActualTouchPosition());
            }
            
            if (_isDragging == true && TouchWrapper.IsFingerDown == false)
            {
                _isDragging = false;
                _sendLong = false;
                DragStop(_clickService.LastFinger0DownPos);
            }
        }

        public void AddMomentum()
        {
            if (_pinchToDragCurrentFrame == false)
            {
                _dragFinalMomentumVector.Add(_touchInputService.GetActualTouchPosition() - _clickService.LastFinger0DownPos);
                if (_dragFinalMomentumVector.Count > MomentumSamplesCount)
                {
                    _dragFinalMomentumVector.RemoveAt(0);
                }
            }
        }

        private float GetRelativeDragDistance(Vector3 pos0, Vector3 pos1)
        {
            Vector2 dragVector = pos0 - pos1;
            float dragDistance = new Vector2(dragVector.x / Screen.width, dragVector.y / Screen.height).magnitude;
            return dragDistance;
        }
        private void DragStart(Vector3 pos, bool isLongTap, bool isInitialDrag)
        {
            OnDragStart?.Invoke(pos, isLongTap);
            _clickService.IsClickPrevented = true;
            _timeSinceDragStart = 0;
            _dragFinalMomentumVector.Clear();
        }

        private void DragUpdate(Vector3 pos)
        {
            _timeSinceDragStart += Time.deltaTime;
            Vector3 offset = Vector3.Lerp(Vector3.zero, _dragStartOffset, Mathf.Clamp01(_timeSinceDragStart * 10.0f));
            OnDragUpdate?.Invoke(_dragStartPos, pos, offset);
        }

        private void DragStop(Vector3 pos)
        {
            if (OnDragStop != null)
            {
                Vector3 momentum = Vector3.zero;
                if (_dragFinalMomentumVector.Count > 0)
                {
                    for (int i = 0; i < _dragFinalMomentumVector.Count; ++i)
                    {
                        momentum += _dragFinalMomentumVector[i];
                    }

                    momentum /= _dragFinalMomentumVector.Count;
                }
                OnDragStop(pos, momentum);
            }
            _dragFinalMomentumVector.Clear();
        }

        
    }
}