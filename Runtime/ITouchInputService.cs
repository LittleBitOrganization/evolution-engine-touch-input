using System;
using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    interface ITouchInputService:IService
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

        public event InputLongTapProgress OnLongTapProgress;

        public Vector3 GetActualTouchPosition();
        public bool LongTapStartsDrag { get; }
        public bool IsInputOnLockedArea { get; set; }
        public bool IsBlockTouch { get; set; }
    }
}