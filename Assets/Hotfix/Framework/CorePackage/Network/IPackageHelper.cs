using System.IO;
using HiSocket.Tcp;

namespace LccHotfix
{
    public interface IPackageHelper
    {
        bool Parse(IBlockBuffer<byte> buffer, out byte[] bytes);
    }
}