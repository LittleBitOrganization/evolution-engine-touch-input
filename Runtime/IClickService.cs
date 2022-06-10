using UnityEngine;

namespace LittleBit.Modules.TouchInput
{
    internal interface IClickService : IClick,IService
    {
        bool IsClickPrevented { get; set; }
        bool WasFingerDownLastFrame { get; set; }
        float LastClickTimeReal { get; set; }
        float LastFingerDownTimeReal { get; set; }
        Vector3 LastFinger0DownPos{ get; set; }
        void FingerDown();
        void Update();
    }
}