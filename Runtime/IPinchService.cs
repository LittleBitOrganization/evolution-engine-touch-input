namespace LittleBit.Modules.TouchInput
{
    internal interface IPinchService:IPinch,IService
    {
        bool IsPinching { get; }
        bool WasPinchingLastFrame { get; set; }
        void Update();
    }
}