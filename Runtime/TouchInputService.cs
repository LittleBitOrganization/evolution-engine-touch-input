using System;
using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    public class TouchInputService : ITouchInputService,IDisposable
    {

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
        
        public event Action OnLongTapProgress;
        
        #region Fields
        
        private readonly TouchInputConfig _touchInputConfig;
        private readonly TouchInputBehavior _touchInputController;
        private readonly IClickService _clickService;
        private readonly IPinchService _pinchService;
        private readonly IDragService _dragService;

        private bool _isInputOnLockedArea = false;
        private bool _isBlockTouch = false;

        #endregion

        
        public TouchInputService(TouchInputConfig touchInputConfig, 
                                 TouchInputBehavior touchInputController)
        {
            _touchInputConfig = touchInputConfig;
            _touchInputController = touchInputController;
            _clickService = new ClickService();
            _pinchService = new PinchService(this,_clickService,_touchInputConfig);
            _dragService = new DragService(this,_clickService,_pinchService,_touchInputConfig);
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _touchInputController.OnUpdate += Update;
            _clickService.OnInputClickDown += FingerDown;
            _clickService.OnInputClickUp += FingerUp;
            _pinchService.OnPinchStart += StartPinch;
            _pinchService.OnPinchStop += StopPinch;
            _pinchService.OnPinchUpdateExtended += UpdatePinch;
            _dragService.OnDragStart += StartDrag;
            _dragService.OnDragUpdate += UpdateDrag;
            _dragService.OnDragStop += StopDrag;
            _dragService.OnLongTapProgress += LongTapProgress;
        }

        public Vector3 GetActualTouchPosition()
        {
            if(_isBlockTouch)
            {
                if(TouchWrapper.TouchCount > 1)
                {
                    _clickService.LastFinger0DownPos = TouchWrapper.Touches[1].Position;
                    return TouchWrapper.Touches[1].Position;
                }
        
                return _clickService.LastFinger0DownPos;
            }
      
            return TouchWrapper.Touch0.Position;
        }

        public bool IsInputOnLockedArea
        {
            get => _isInputOnLockedArea;
            set => _isInputOnLockedArea = value;
        }

        public bool IsBlockTouch
        {
            get => _isBlockTouch;
            set => _isBlockTouch = value;
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
            if (!_isInputOnLockedArea)
            {
                _pinchService.Update();
                
                if (_pinchService.IsPinching == false)
                {
                    _dragService.Update();    
                }

                #region click
                if (_pinchService.IsPinching == false &&
                    _dragService.IsDragging == false && 
                    _pinchService.WasPinchingLastFrame == false && 
                    _dragService.WasDraggingLastFrame == false && 
                    _clickService.IsClickPrevented == false)
                {
                    if (_clickService.WasFingerDownLastFrame == false && TouchWrapper.IsFingerDown)
                    {
                        _clickService.LastFingerDownTimeReal = Time.realtimeSinceStartup;
                        _dragService.StartDragPosition = GetActualTouchPosition();
                        _clickService.FingerDown();
                    }

                    if (_clickService.WasFingerDownLastFrame == true && TouchWrapper.IsFingerDown == false)
                    {
                        float fingerDownUpDuration = Time.realtimeSinceStartup - _clickService.LastFingerDownTimeReal;

                        if (_dragService.WasDraggingLastFrame == false && _pinchService.WasPinchingLastFrame == false)
                        {
                            float clickDuration = Time.realtimeSinceStartup - _clickService.LastClickTimeReal;

                            bool isDoubleClick = clickDuration < _touchInputConfig.DoubleclickDurationThreshold;
                            bool isLongTap = fingerDownUpDuration > _touchInputConfig.ClickDurationThreshold;
                            
                            OnInputClick?.Invoke(_clickService.LastFinger0DownPos, isDoubleClick, isLongTap);
                            
                            _clickService.LastClickTimeReal = Time.realtimeSinceStartup;
                        }
                    }
                }
                #endregion
            }

            if (_dragService.IsDragging && TouchWrapper.IsFingerDown)
            {
                _dragService.AddMomentum();
            }

            if (_isInputOnLockedArea == false)
            {
                _clickService.WasFingerDownLastFrame = TouchWrapper.IsFingerDown;
            }

            if (_clickService.WasFingerDownLastFrame == true)
            {
                _clickService.LastFinger0DownPos = GetActualTouchPosition();
            }

            _clickService.Update();
#if UNITY_EDITOR || UNITY_STANDALONE
            
            float mouseScrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (mouseScrollDelta!=0)
                OnMouseZoom?.Invoke(mouseScrollDelta);
#endif 
        }

        public void Dispose()
        {
            _touchInputController.OnUpdate -= Update;
            _clickService.OnInputClickDown -= FingerDown;
            _clickService.OnInputClickUp -= FingerUp;
            _pinchService.OnPinchStart -= StartPinch;
            _pinchService.OnPinchStop -= StopPinch;
            _pinchService.OnPinchUpdateExtended -= UpdatePinch;
            _dragService.OnDragStart -= StartDrag;
            _dragService.OnDragUpdate -= UpdateDrag;
            _dragService.OnDragStop -= StopDrag;
            _dragService.OnLongTapProgress -= LongTapProgress;
        }

        private void FingerUp(Vector3 pos)
        {
            OnInputClickUp?.Invoke(pos);
        }
        private void FingerDown(Vector3 pos)
        {
            OnInputClickDown?.Invoke(pos);
        }

        private void UpdatePinch(PinchUpdateData pinchupdatedata)
        {
            OnPinchUpdateExtended?.Invoke(pinchupdatedata);
        }
        private void StopPinch()
        {
            _dragService.DragStartOffset = Vector3.zero;
            OnPinchStop?.Invoke();
        }
        private void StartPinch(Vector3 pinchcenter, float pinchdistance)
        {
            OnPinchStart?.Invoke(pinchcenter,pinchdistance);
        }
        private void StopDrag(Vector3 dragstoppos, Vector3 dragfinalmomentum)
        {
            OnDragStop?.Invoke(dragstoppos,dragfinalmomentum);
        }

        private void UpdateDrag(Vector3 dragposstart, Vector3 dragposcurrent, Vector3 correctionoffset)
        {
            OnDragUpdate?.Invoke(dragposstart,dragposcurrent,correctionoffset);
        }

        private void StartDrag(Vector3 pos, bool islongtap)
        {
            OnDragStart?.Invoke(pos,islongtap);
        }
        private void LongTapProgress()
        {
            OnLongTapProgress?.Invoke();
        }
    }
}