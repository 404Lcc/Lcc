public class SendTool
{
    private static SendTool _instance;
    public static SendTool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SendTool();
            }
            return _instance;
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