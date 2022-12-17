namespace LccModel
{
    public interface INumericEvent
    {
        void Publish(long oldValue, long newValue);
    }
}