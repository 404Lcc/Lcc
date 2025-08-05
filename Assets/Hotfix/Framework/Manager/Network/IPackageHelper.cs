using System.IO;
using HiSocket.Tcp;

public interface IPackageHelper
{
     bool Parse(IBlockBuffer<byte> buffer, out byte[] bytes);
}