namespace LccModel
{
    public class EventData
    {
        public IEvent IEvent { get; }

        public EventData(IEvent iEvent)
        {
            IEvent = iEvent;
        }
    }
}