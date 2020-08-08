public class SendTool
{
    private static SendTool instance;
    public static SendTool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SendTool();
            }
            return instance;
        }
    }
    public void Send(ClientNet net, byte type, int area, int command, object message)
    {
        SocketModel model = new SocketModel(type, area, command, message);
        byte[] data = EncodingTool.Instance.SocketModelEncode(model);
        data = EncodingTool.Instance.LengthEncode(data);
        net.Send(data);
    }
}