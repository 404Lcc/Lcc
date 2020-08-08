public class SocketModel
{
    public byte type;
    public int area;
    public int command;
    public object message;
    public SocketModel()
    {
    }
    public SocketModel(byte type, int area, int command, object message)
    {
        this.type = type;
        this.area = area;
        this.command = command;
        this.message = message;
    }
}