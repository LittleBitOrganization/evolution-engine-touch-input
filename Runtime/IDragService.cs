using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    internal interface IDragService : IDrag,ILongTap, IService
    {
        bool IsDragging { get; }
        Vector3 StartDragPosition { get; set; }
        Vector3 DragStartOffset { get; set; }
        bool WasDraggingLastFrame{ get; set; }
        void Update();
        void AddMomentum();
    }
}