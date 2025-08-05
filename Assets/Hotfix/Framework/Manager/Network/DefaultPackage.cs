using System.IO;
using HiSocket.Tcp;

public class DefaultPackage : IPackage
{
    private IPackageHelper _packageHelper;

    public void SetPackageHelper(IPackageHelper packageHelper)
    {
        _packageHelper = packageHelper;
    }

    public void Pack(byte[] message, IBlockBuffer<byte> sendBuffer)
    {
        sendBuffer.WriteAtEnd(message);
    }

    public void Unpack(IBlockBuffer<byte> receiveBuffer, ref byte[] message)
    {
        if (_packageHelper.Parse(receiveBuffer, out var bytes))
        {
            message = bytes;
        }
    }
}