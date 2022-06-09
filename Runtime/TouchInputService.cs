using System;
using System.Collections.Generic;
using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    public class TouchInputService : ITouchInputService,IDisposable
    {

        #region Consts

        private const float DragDurationThreshold = 0.01f;
        private const int MomentumSamplesCount = 5;

        #endregion

        #region Fields
        
        private readonly TouchInputConfig _touchInputConfig;
        private readonly TouchInputBehavior _touchInputController;

        private float _lastFingerDownTimeReal = 0;
        
        private float _lastClickTimeReal = 0;
        
        private bool _wasFingerDownLastFrame = false;
        
        private Vector3 _lastFinger0DownPos = Vector3.zero;
        
        private Vector3 _lastFinger1DownPos;
        
        private bool _isDragging = false;
        
        private Vector3 _dragStartPos = Vector3.zero;
        
        private Vector3 _dragStartOffset;
        
        private List<Vector3> _dragFinalMomentumVector = new List<Vector3>();
        
        private float _pinchStartDistance = 1;
        
        private List<Vector3> _pinchStartPositions = new List<Vector3>() {Vector3.zero, Vector3.zero};
        
        private List<Vector3> _touchPositionLastFrame = new List<Vector3>() {Vector3.zero, Vector3.zero};
        
        private Vector3 _pinchRotationVectorStart = Vector3.zero;
        
        private Vector3 _pinchVectorLastFrame = Vector3.zero;
        
        private float _totalFingerMovement;
        
        private bool _wasDraggingLastFrame = false;
        private bool _wasPinchingLastFrame = false;

        private bool _isPinching = false;

        private bool _isInputOnLockedArea = false;
        private bool _isBlockTouch = false;

        private float _timeSinceDragStart = 0;
        
        private bool _isClickPrevented = false;
        
        private bool _isFingerDown = false;
        #endregion

        public TouchInputService(TouchInputConfig touchInputConfig, TouchInputBehavior touchInputController)
        {
            _touchInputConfig = touchInputConfig;
            _touchInputController = touchInputController;
            _touchInputController.OnUpdate += Update;
        }
        private void Update()
        {
            if (!TouchWrapper.IsFingerDown)
            {
                _isInputOnLockedArea = false;
        
                if (TouchWrapper.TouchCount == 0 && _isBlockTouch)
                {
                    _isBlockTouch = false;
                }
            }

            bool pinchToDragCurrentFrame = false;

            if (!_isInputOnLockedArea)
            {
                #region pinch
                if (!_isBlockTouch || (_isBlockTouch && TouchWrapper.TouchCount > 2))
                {
                    if (_isPinching == false)
                    {
                        if (TouchWrapper.TouchCount == 2)
                        {
                            StartPinch();
                            _isPinching = true;
                        }
                    }
                    else
                    {
                        if (TouchWrapper.TouchCount < 2)
                        {
                            StopPinch();
                            _isPinching = false;
                        }
                        else if (TouchWrapper.TouchCount == 2)
                        {
                            UpdatePinch();
                        }
                    }
                }
                #endregion

                #region drag
                if (_isPinching == false)
                {
                    if (_wasPinchingLastFrame == false)
                    {
                        if (_wasFingerDownLastFrame == true && TouchWrapper.IsFingerDown)
                        {
                            if (_isDragging == false)
                            {
                                float dragDistance = GetRelativeDragDistance(GetActualTouchPosition(), _dragStartPos);
                                float dragTime = Time.realtimeSinceStartup - _lastFingerDownTimeReal;

                                bool isLongTap = dragTime > _touchInputConfig.ClickDurationThreshold;
                                if (OnLongTapProgress != null)
                                {
                                    float longTapProgress = 0;
                                    if (Mathf.Approximately(_touchInputConfig.ClickDurationThreshold, 0) == false)
                                    {
                                        longTapProgress = Mathf.Clamp01(dragTime / _touchInputConfig.ClickDurationThreshold);
                                    }

                                    OnLongTapProgress(longTapProgress);
                                }

                                if ((dragDistance >= _touchInputConfig.DragStartDistanceThresholdRelative && dragTime >= DragDurationThreshold)
                                    || (_touchInputConfig.LongTapStartsDrag == true && isLongTap == true))
                                {
                                    _isDragging = true;
                                    _dragStartOffset = _lastFinger0DownPos - _dragStartPos;
                                    _dragStartPos = _lastFinger0DownPos;
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
                            _dragStartPos = GetActualTouchPosition();
                            DragStart(_dragStartPos, false, false);
                            pinchToDragCurrentFrame = true;
                        }
                    }

                    if (_isDragging == true && TouchWrapper.IsFingerDown == true)
                    {
                        DragUpdate(GetActualTouchPosition());
                    }

                    if (_isDragging == true && TouchWrapper.IsFingerDown == false)
                    {
                        _isDragging = false;
                        DragStop(_lastFinger0DownPos);
                    }
                }
                #endregion

                #region click
                if (_isPinching == false && _isDragging == false && _wasPinchingLastFrame == false && _wasDraggingLastFrame == false && _isClickPrevented == false)
                {
                    if (_wasFingerDownLastFrame == false && TouchWrapper.IsFingerDown)
                    {
                        _lastFingerDownTimeReal = Time.realtimeSinceStartup;
                        _dragStartPos = GetActualTouchPosition();
                        FingerDown(TouchWrapper.AverageTouchPos);
                    }

                    if (_wasFingerDownLastFrame == true && TouchWrapper.IsFingerDown == false)
                    {
                        float fingerDownUpDuration = Time.realtimeSinceStartup - _lastFingerDownTimeReal;

                        if (_wasDraggingLastFrame == false && _wasPinchingLastFrame == false)
                        {
                            float clickDuration = Time.realtimeSinceStartup - _lastClickTimeReal;

                            bool isDoubleClick = clickDuration < _touchInputConfig.DoubleclickDurationThreshold;
                            bool isLongTap = fingerDownUpDuration > _touchInputConfig.ClickDurationThreshold;

                            if (OnInputClick != null)
                            {
                                OnInputClick.Invoke(_lastFinger0DownPos, isDoubleClick, isLongTap);
                            }

                            _lastClickTimeReal = Time.realtimeSinceStartup;
                        }
                    }
                }
                #endregion
            }

            if (_isDragging && TouchWrapper.IsFingerDown && pinchToDragCurrentFrame == false)
            {
                _dragFinalMomentumVector.Add(GetActualTouchPosition() - _lastFinger0DownPos);
                if (_dragFinalMomentumVector.Count > MomentumSamplesCount)
                {
                    _dragFinalMomentumVector.RemoveAt(0);
                }
            }

            if (_isInputOnLockedArea == false)
            {
                _wasFingerDownLastFrame = TouchWrapper.IsFingerDown;
            }

            if (_wasFingerDownLastFrame == true)
            {
                _lastFinger0DownPos = GetActualTouchPosition();
            }

            _wasDraggingLastFrame = _isDragging;
            _wasPinchingLastFrame = _isPinching;

            if (TouchWrapper.TouchCount == 0)
            {
                _isClickPrevented = false;
                if (_isFingerDown == true)
                {
                    FingerUp(TouchWrapper.AverageTouchPos);
                }
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            float mouseScrollDelta = Input.GetAxis("Mouse ScrollWheel");
            OnMouseZoom?.Invoke(mouseScrollDelta);
#endif 
        }
        private void StartPinch()
        {
            _pinchStartPositions[0] = _touchPositionLastFrame[0] = TouchWrapper.Touches[0].Position;
            _pinchStartPositions[1] = _touchPositionLastFrame[1] = TouchWrapper.Touches[1].Position;

            _pinchStartDistance = GetPinchDistance(_pinchStartPositions[0], _pinchStartPositions[1]);

            OnPinchStart?.Invoke((_pinchStartPositions[0] + _pinchStartPositions[1]) * 0.5f, _pinchStartDistance);

            _isClickPrevented = true;
            _pinchRotationVectorStart = TouchWrapper.Touches[1].Position - TouchWrapper.Touches[0].Position;
            _pinchVectorLastFrame = _pinchRotationVectorStart;
            _totalFingerMovement = 0;
        }

        private void UpdatePinch()
        {
            float pinchDistance = GetPinchDistance(TouchWrapper.Touches[0].Position, TouchWrapper.Touches[1].Position);
            Vector3 pinchVector = TouchWrapper.Touches[1].Position - TouchWrapper.Touches[0].Position;
            float pinchAngleSign = Vector3.Cross(_pinchVectorLastFrame, pinchVector).z < 0 ? -1 : 1;
            float pinchAngleDelta = 0;
            if (Mathf.Approximately(Vector3.Distance(_pinchVectorLastFrame, pinchVector), 0) == false)
            {
                pinchAngleDelta = Vector3.Angle(_pinchVectorLastFrame, pinchVector) * pinchAngleSign;
            }

            float pinchVectorDeltaMag = Mathf.Abs(_pinchVectorLastFrame.magnitude - pinchVector.magnitude);
            float pinchAngleDeltaNormalized = 0;
            if (Mathf.Approximately(pinchVectorDeltaMag, 0) == false)
            {
                pinchAngleDeltaNormalized = pinchAngleDelta / pinchVectorDeltaMag;
            }

            Vector3 pinchCenter = (TouchWrapper.Touches[0].Position + TouchWrapper.Touches[1].Position) * 0.5f;

            #region tilting gesture

            float pinchTiltDelta = 0;
            Vector3 touch0DeltaRelative =
                GetTouchPositionRelative(TouchWrapper.Touches[0].Position - _touchPositionLastFrame[0]);
            Vector3 touch1DeltaRelative =
                GetTouchPositionRelative(TouchWrapper.Touches[1].Position - _touchPositionLastFrame[1]);
            
            float touch0DotUp = Vector2.Dot(touch0DeltaRelative.normalized, Vector2.up);
            float touch1DotUp = Vector2.Dot(touch1DeltaRelative.normalized, Vector2.up);
            float pinchVectorDotHorizontal = Vector3.Dot(pinchVector.normalized, Vector3.right);
            if (Mathf.Sign(touch0DotUp) == Mathf.Sign(touch1DotUp))
            {
                if (Mathf.Abs(touch0DotUp) > _touchInputConfig.TiltMoveDotTreshold && 
                    Mathf.Abs(touch1DotUp) > _touchInputConfig.TiltMoveDotTreshold)
                {
                    if (Mathf.Abs(pinchVectorDotHorizontal) >= _touchInputConfig.TiltHorizontalDotThreshold)
                    {
                        pinchTiltDelta = 0.5f * (touch0DeltaRelative.y + touch1DeltaRelative.y);
                    }
                }
            }

            _totalFingerMovement += touch0DeltaRelative.magnitude + touch1DeltaRelative.magnitude;

            #endregion

            OnPinchUpdateExtended?.Invoke(new PinchUpdateData() 
            {
                pinchCenter = pinchCenter, pinchDistance = pinchDistance, pinchStartDistance = _pinchStartDistance,
                pinchAngleDelta = pinchAngleDelta, pinchAngleDeltaNormalized = pinchAngleDeltaNormalized,
                pinchTiltDelta = pinchTiltDelta, pinchTotalFingerMovement = _totalFingerMovement
            });
            

            _pinchVectorLastFrame = pinchVector;
            _touchPositionLastFrame[0] = TouchWrapper.Touches[0].Position;
            _touchPositionLastFrame[1] = TouchWrapper.Touches[1].Position;

        }

        private float GetPinchDistance(Vector3 pos0, Vector3 pos1)
        {
            float distanceX = Mathf.Abs(pos0.x - pos1.x) / Screen.width;
            float distanceY = Mathf.Abs(pos0.y - pos1.y) / Screen.height;
            return (Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY));
        }

        private void StopPinch()
        {
            _dragStartOffset = Vector3.zero;
            OnPinchStop?.Invoke();
        }

        private void DragStart(Vector3 pos, bool isLongTap, bool isInitialDrag)
        {
            OnDragStart?.Invoke(pos, isLongTap);
            _isClickPrevented = true;
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

        private void FingerDown(Vector3 pos)
        {
            _isFingerDown = true;
            OnInputClickDown?.Invoke(pos);
        }

        private void FingerUp(Vector3 pos)
        {
            _isFingerDown = false;
            OnInputClickUp?.Invoke(pos);
        }

        private Vector3 GetTouchPositionRelative(Vector3 touchPosScreen)
        {
            return (new Vector3(touchPosScreen.x / (float) Screen.width, touchPosScreen.y / (float) Screen.height,
                touchPosScreen.z));
        }

        private float GetRelativeDragDistance(Vector3 pos0, Vector3 pos1)
        {
            Vector2 dragVector = pos0 - pos1;
            float dragDistance = new Vector2(dragVector.x / Screen.width, dragVector.y / Screen.height).magnitude;
            return dragDistance;
        }

        public void Dispose()
        {
            _touchInputController.OnUpdate -= Update;
        }

        public event InputClickDelegate OnInputClick;
        public event InputPositionDelegate OnInputClickUp;
        public event InputPositionDelegate OnInputClickDown;
        
        public event InputDragStartDelegate OnDragStart;
        public event DragUpdateDelegate OnDragUpdate;
        public event DragStopDelegate OnDragStop;
        
        public event PinchStartDelegate OnPinchStart;
        public event PinchUpdateExtendedDelegate OnPinchUpdateExtended;
        public event Action OnPinchStop;
        public event InputMouseZoomAmount OnMouseZoom;

        public event InputLongTapProgress OnLongTapProgress;
        
        public Vector3 GetActualTouchPosition()
        {
            if(_isBlockTouch)
            {
                if(TouchWrapper.TouchCount > 1)
                {
                    _lastFinger0DownPos = TouchWrapper.Touches[1].Position;
                    return TouchWrapper.Touches[1].Position;
                }
        
                return _lastFinger0DownPos;
            }
      
            return TouchWrapper.Touch0.Position;
        }
        
        public bool LongTapStartsDrag
        {
            get { return _touchInputConfig.LongTapStartsDrag; }
        }

        public bool IsInputOnLockedArea
        {
            get { return _isInputOnLockedArea; }
            set { _isInputOnLockedArea = value; }
        }

        public bool IsBlockTouch
        {
            get { return _isBlockTouch; }
            set { _isBlockTouch = value; }
        }
    }
}