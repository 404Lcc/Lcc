namespace LccHotfix
{
    public interface IMessageHelper
    {
        byte[] GetBytes(int code, object message);
        LccHotfix.NetworkMessage MessageParse(byte[] bytes);
    }
}