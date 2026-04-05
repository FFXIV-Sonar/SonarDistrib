namespace Sonar.Data.Rows
{
    public static class EventExtensions
    {
        extension (EventRow ev)
        {
            public EventType Type => EventUtils.TypeFromId(ev.Id);
            public uint RowId => EventUtils.FromId(ev.Id).RowId;
            public uint SubRowId => EventUtils.FromId(ev.Id).SubRowId;
        }
    }
}
