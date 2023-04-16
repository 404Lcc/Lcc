using System;
using System.IO;
using System.Net;

namespace LccHotfix
{
	public struct Packet
	{
		public const int MinPacketSize = 2;
		public const int OpcodeIndex = 4;
		public const int OpcodeLength = 4;

		public MemoryStream MemoryStream;
	}

	public abstract class AChannel: IDisposable
	{
		public long Id;
		
		public int Error { get; set; }
		
		public IPEndPoint RemoteAddress { get; set; }

		
		public bool IsDisposed
		{
			get
			{
				return this.Id == 0;	
			}
		}

		public abstract void Dispose();
	}
}