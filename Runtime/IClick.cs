namespace LittleBit.Modules.TouchInput
{
    internal interface IClick
    {
        event InputClickDelegate OnInputClick;
        event InputPositionDelegate OnInputClickUp;
        event InputPositionDelegate OnInputClickDown;
    }
}