public interface IMessageHelper
{
    byte[] GetBytes(object message);
    NetworkMessage MessageParse(byte[] bytes);
}