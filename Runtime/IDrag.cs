namespace LittleBit.Modules.TouchInput
{
    internal interface IDrag
    {
        event InputDragStartDelegate OnDragStart;
        event DragUpdateDelegate OnDragUpdate;
        event DragStopDelegate OnDragStop;
    }
}