using System.IO;
using HiSocket.Tcp;

namespace LccHotfix
{
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

        public bool Unpack(IBlockBuffer<byte> receiveBuffer, out byte[] message)
        {
            if (_packageHelper.Parse(receiveBuffer, out message))
            {
                return true;
            }

            return false;
        }
    }
}