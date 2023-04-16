using System;
using System.IO;

namespace LccHotfix
{
	public enum ParserState
	{
		PacketSize,
		PacketBody
	}

	public class PacketParser
	{
		private readonly CircularBuffer buffer;
		private int packetSize;
		private ParserState state;
		public AService service;
		private readonly byte[] cache = new byte[8];
		public const int OuterPacketSizeLength = 4;
		public MemoryStream MemoryStream;

		public PacketParser(CircularBuffer buffer, AService service)
		{
			this.buffer = buffer;
			this.service = service;
		}

		public bool Parse()
		{
			while (true)
			{
				switch (this.state)
				{
					case ParserState.PacketSize:
						{
							if (this.buffer.Length < OuterPacketSizeLength)
							{
								return false;
							}

							this.buffer.Read(this.cache, 0, OuterPacketSizeLength);

							this.packetSize = BitConverter.ToInt32(this.cache, 0);
							if (this.packetSize < Packet.MinPacketSize)
							{
								throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
							}

							this.state = ParserState.PacketBody;
							break;
						}
					case ParserState.PacketBody:
						{
							if (this.buffer.Length < this.packetSize)
							{
								return false;
							}

							MemoryStream memoryStream = new MemoryStream(this.packetSize);
							this.buffer.Read(memoryStream, this.packetSize);
							//memoryStream.SetLength(this.packetSize - Packet.MessageIndex);
							this.MemoryStream = memoryStream;

							memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);

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