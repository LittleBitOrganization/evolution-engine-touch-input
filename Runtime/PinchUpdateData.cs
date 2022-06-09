using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    public class PinchUpdateData {
        public Vector3 pinchCenter;
        public float pinchDistance;
        public float pinchStartDistance;
        public float pinchAngleDelta;
        public float pinchAngleDeltaNormalized;
        public float pinchTiltDelta;
        public float pinchTotalFingerMovement;
    }
}