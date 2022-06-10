using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    public delegate void InputClickDelegate(Vector3 clickPosition, bool isDoubleClick, bool isLongTap);
    public delegate void InputDragStartDelegate(Vector3 pos, bool isLongTap);
    public delegate void DragUpdateDelegate(Vector3 dragPosStart, Vector3 dragPosCurrent, Vector3 correctionOffset);
    public delegate void DragStopDelegate(Vector3 dragStopPos, Vector3 dragFinalMomentum);
    public delegate void PinchStartDelegate(Vector3 pinchCenter, float pinchDistance);
    public delegate void PinchUpdateExtendedDelegate(PinchUpdateData pinchUpdateData);
    public delegate void InputPositionDelegate(Vector3 pos);
    public delegate void InputMouseZoomAmount(float progress);
}