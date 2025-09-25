using System;
using HiSocket.Tcp;

namespace LccHotfix
{
    public class DefaultPackageHelper : IPackageHelper
    {
        public enum ParserState
        {
            PacketSize,
            PacketBody
        }

        private IBlockBuffer<byte> buffer;
        private ParserState state;
        private int packetSize;

        //消息拆包处理
        public bool Parse(IBlockBuffer<byte> buffer, out byte[] bytes)
        {
            this.buffer = buffer;
            bytes = new byte[] { };
            while (true)
            {
                switch (state)
                {
                    case ParserState.PacketSize:
                    {
                        if (this.buffer.Index < 4)
                        {
                            return false;
                        }

                        //消息号长度+消息体长度
                        this.packetSize = BitConverter.ToInt32(buffer.ReadFromHead(4));
                        this.state = ParserState.PacketBody;
                        break;
                    }
                    case ParserState.PacketBody:
                    {
                        if (this.buffer.Index < this.packetSize)
                        {
                            return false;
                        }

                        //消息号+消息体
                        bytes = buffer.ReadFromHead(packetSize);
                        this.state = ParserState.PacketSize;
                        return true;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}