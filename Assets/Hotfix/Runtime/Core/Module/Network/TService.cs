using LccModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace LccHotfix
{
	public enum TcpOp
	{
		StartSend,
		StartRecv,
		Connect,
	}
	
	public struct TArgs
	{
		public TcpOp Op;
		public long ChannelId;
		public SocketAsyncEventArgs SocketAsyncEventArgs;
	}
	
	public sealed class TService : AService
	{
		private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();

		public ConcurrentQueue<TArgs> Queue = new ConcurrentQueue<TArgs>();

		private TChannel Create(IPEndPoint ipEndPoint, long id)
		{
			TChannel channel = new TChannel(id, ipEndPoint, this);
			this.idChannels.Add(channel.Id, channel);
			return channel;
		}

		public override void Create(long id, IPEndPoint address)
		{
			if (this.idChannels.TryGetValue(id, out TChannel _))
			{
				return;
			}
			this.Create(address, id);
		}
		
		private TChannel Get(long id)
		{
			TChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}
		
		public override void Dispose()
		{
			base.Dispose();

			foreach (long id in this.idChannels.Keys.ToArray())
			{
				TChannel channel = this.idChannels[id];
				channel.Dispose();
			}
			this.idChannels.Clear();
		}

		public override void Remove(long id, int error = 0)
		{
			if (this.idChannels.TryGetValue(id, out TChannel channel))
			{
				channel.Error = error;
				channel.Dispose();	
			}
			this.idChannels.Remove(id);
		}

		public override void Send(long channelId, object message)
		{
			try
			{
				TChannel aChannel = this.Get(channelId);
				if (aChannel == null)
				{
					NetServices.Instance.OnError(this.Id, channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
					return;
				}
				MemoryStream memoryStream = this.GetMemoryStream(message);
				aChannel.Send(memoryStream);
			}
			catch (Exception e)
			{
                LogUtil.Error(e);
			}
		}
		
		public override void Update()
		{
			while (true)
			{
				if (!this.Queue.TryDequeue(out var result))
				{
					break;
				}
				
				SocketAsyncEventArgs e = result.SocketAsyncEventArgs;

				if (e == null)
				{
					switch (result.Op)
					{
						case TcpOp.StartSend:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.StartSend();
							}
							break;
						}
						case TcpOp.StartRecv:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.StartRecv();
							}
							break;
						}
						case TcpOp.Connect:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.ConnectAsync();
							}
							break;
						}
					}
					continue;
				}

				switch (e.LastOperation)
				{
					case SocketAsyncOperation.Connect:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnConnectComplete(e);
						}

						break;
					}
					case SocketAsyncOperation.Disconnect:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnDisconnectComplete(e);
						}
						break;
					}
					case SocketAsyncOperation.Receive:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnRecvComplete(e);
						}
						break;
					}
					case SocketAsyncOperation.Send:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnSendComplete(e);
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException($"{e.LastOperation}");
				}
			}
		}
		
		public override bool IsDispose()
		{
			return false;
		}
	}
}