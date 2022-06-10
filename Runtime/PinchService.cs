using System;
using System.Collections.Generic;
using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    internal class PinchService : IPinchService
    {
        private readonly ITouchInputService _touchInputService;
        private readonly IClickService _clickService;
        private readonly TouchInputConfig _touchInputConfig;
        private bool _isPinching = false;
        private float _pinchStartDistance = 1;
        private float _totalFingerMovement;
        private List<Vector3> _pinchStartPositions = new List<Vector3>() {Vector3.zero, Vector3.zero};
        private List<Vector3> _touchPositionLastFrame = new List<Vector3>() {Vector3.zero, Vector3.zero};
        private bool _wasPinchingLastFrame = false;
        
        private Vector3 _pinchRotationVectorStart = Vector3.zero;
        private Vector3 _pinchVectorLastFrame = Vector3.zero;
        
        public bool IsPinching => _isPinching;
        
        public event PinchStartDelegate OnPinchStart;
        public event PinchUpdateExtendedDelegate OnPinchUpdateExtended;
        public event Action OnPinchStop;
        
        public PinchService(ITouchInputService touchInputService,IClickService clickService,TouchInputConfig touchInputConfig)
        {
            _touchInputService = touchInputService;
            _clickService = clickService;
            _touchInputConfig = touchInputConfig;
        }
        public bool WasPinchingLastFrame
        {
            get => _wasPinchingLastFrame;
            set => _wasPinchingLastFrame = value;
        }
        public void Update()
        {
            if (!_touchInputService.IsBlockTouch || (_touchInputService.IsBlockTouch && TouchWrapper.TouchCount > 2))
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
        }
        
        private void StartPinch()
        {
            _pinchStartPositions[0] = _touchPositionLastFrame[0] = TouchWrapper.Touches[0].Position;
            _pinchStartPositions[1] = _touchPositionLastFrame[1] = TouchWrapper.Touches[1].Position;

            _pinchStartDistance = GetPinchDistance(_pinchStartPositions[0], _pinchStartPositions[1]);

            OnPinchStart?.Invoke((_pinchStartPositions[0] + _pinchStartPositions[1]) * 0.5f, _pinchStartDistance);

            _clickService.IsClickPrevented = true;
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

        private Vector3 GetTouchPositionRelative(Vector3 touchPosScreen)
        {
            return (new Vector3(touchPosScreen.x / (float) Screen.width, touchPosScreen.y / (float) Screen.height,
                touchPosScreen.z));
        }
        
        private void StopPinch()
        {
            OnPinchStop?.Invoke();
        }
        
        
        
    }
}