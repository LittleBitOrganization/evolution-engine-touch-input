using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    interface ITouchInputService:IPinch,IDrag,ILongTap,IClick,IService
    {
        public event InputMouseZoomAmount OnMouseZoom;
        public Vector3 GetActualTouchPosition();
        public bool IsInputOnLockedArea { get; set; }
        public bool IsBlockTouch { get; set; }
    }
}