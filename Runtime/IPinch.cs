using System;

namespace LittleBit.Modules.TouchInput
{
    internal interface IPinch
    {
        event PinchStartDelegate OnPinchStart;
        event PinchUpdateExtendedDelegate OnPinchUpdateExtended;
        event Action OnPinchStop;
    }
}