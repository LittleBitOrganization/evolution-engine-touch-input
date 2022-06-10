using System;

namespace LittleBit.Modules.TouchInput
{
    internal interface ILongTap
    {
        event Action OnLongTapProgress;
    }
}