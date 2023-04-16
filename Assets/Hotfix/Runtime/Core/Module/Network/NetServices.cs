using LccModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace LccHotfix
{
    public enum NetOp : byte
    {
        AddService = 1,
        RemoveService = 2,
        OnRead = 3,
        OnError = 4,
        CreateChannel = 5,
        RemoveChannel = 6,
        SendMessage = 7,
    }

    public struct NetOperator
    {
        public NetOp Op; // 操作码
        public int ServiceId;
        public long ChannelId;
        public object Object; // 参数
    }

    public class NetServices : Singleton<NetServices>
    {
        private readonly ConcurrentQueue<NetOperator> netThreadOperators = new ConcurrentQueue<NetOperator>();
        private readonly ConcurrentQueue<NetOperator> mainThreadOperators = new ConcurrentQueue<NetOperator>();

        public NetServices()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypesByAttribute(typeof(MessageAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
                if (messageAttribute == null)
                {
                    continue;
                }

                this.typeOpcode.Add(type, messageAttribute.Opcode);
            }
        }

        #region 线程安全

        // 初始化后不变，所以主线程，网络线程都可以读
        private readonly DoubleMap<Type, int> typeOpcode = new DoubleMap<Type, int>();

        public int GetOpcode(Type type)
        {
            return this.typeOpcode.GetValueByKey(type);
        }

        public Type GetType(int opcode)
        {
            return this.typeOpcode.GetKeyByValue(opcode);
        }

        #endregion



        #region 主线程

        private readonly Dictionary<int, Action<long, object>> readCallback = new Dictionary<int, Action<long, object>>();
        private readonly Dictionary<int, Action<long, int>> errorCallback = new Dictionary<int, Action<long, int>>();

        private int serviceIdGenerator;

        public void SendMessage(int serviceId, long channelId, object message)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.SendMessage, ServiceId = serviceId, ChannelId = channelId, Object = message };
            this.netThreadOperators.Enqueue(netOperator);
        }

        public int AddService(AService aService)
        {
            aService.Id = ++this.serviceIdGenerator;
            NetOperator netOperator = new NetOperator() { Op = NetOp.AddService, ServiceId = aService.Id, ChannelId = 0, Object = aService };
            this.netThreadOperators.Enqueue(netOperator);
            return aService.Id;
        }

        public void RemoveService(int serviceId)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.RemoveService, ServiceId = serviceId };
            this.netThreadOperators.Enqueue(netOperator);
        }

        public void RemoveChannel(int serviceId, long channelId, int error)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.RemoveChannel, ServiceId = serviceId, ChannelId = channelId, Object = error };
            this.netThreadOperators.Enqueue(netOperator);
        }

        public void CreateChannel(int serviceId, long channelId, IPEndPoint address)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.CreateChannel, ServiceId = serviceId, ChannelId = channelId, Object = address };
            this.netThreadOperators.Enqueue(netOperator);
        }

        public void RegisterReadCallback(int serviceId, Action<long, object> action)
        {
            this.readCallback.Add(serviceId, action);
        }

        public void RegisterErrorCallback(int serviceId, Action<long, int> action)
        {
            this.errorCallback.Add(serviceId, action);
        }

        public void UpdateInMainThread()
        {
            while (true)
            {
                if (!this.mainThreadOperators.TryDequeue(out NetOperator op))
                {
                    return;
                }

                try
                {
                    switch (op.Op)
                    {
                        case NetOp.OnRead:
                            {
                                if (!this.readCallback.TryGetValue(op.ServiceId, out var action))
                                {
                                    return;
                                }
                                action.Invoke(op.ChannelId, op.Object);
                                break;
                            }
                        case NetOp.OnError:
                            {
                                if (!this.errorCallback.TryGetValue(op.ServiceId, out var action))
                                {
                                    return;
                                }

                                action.Invoke(op.ChannelId, (int)op.Object);
                                break;
                            }
                        default:
                            throw new Exception($"not found net operator: {op.Op}");
                    }
                }
                catch (Exception e)
                {
                    LogUtil.Error(e);
                }
            }
        }

        #endregion

        #region 网络线程

        private readonly Dictionary<int, AService> services = new Dictionary<int, AService>();
        private readonly Queue<int> queue = new Queue<int>();

        private void Add(AService aService)
        {
            this.services[aService.Id] = aService;
            this.queue.Enqueue(aService.Id);
        }

        public AService Get(int id)
        {
            AService aService;
            this.services.TryGetValue(id, out aService);
            return aService;
        }

        private void Remove(int id)
        {
            if (this.services.Remove(id, out AService service))
            {
                service.Dispose();
            }
        }

        private void RunNetThreadOperator()
        {
            while (true)
            {
                if (!this.netThreadOperators.TryDequeue(out NetOperator op))
                {
                    return;
                }

                try
                {
                    switch (op.Op)
                    {
                        case NetOp.AddService:
                            {
                                this.Add(op.Object as AService);
                                break;
                            }
                        case NetOp.RemoveService:
                            {
                                this.Remove(op.ServiceId);
                                break;
                            }
                        case NetOp.CreateChannel:
                            {
                                AService service = this.Get(op.ServiceId);
                                if (service != null)
                                {
                                    service.Create(op.ChannelId, op.Object as IPEndPoint);
                                }
                                break;
                            }
                        case NetOp.RemoveChannel:
                            {
                                AService service = this.Get(op.ServiceId);
                                if (service != null)
                                {
                                    service.Remove(op.ChannelId, (int)op.Object);
                                }
                                break;
                            }
                        case NetOp.SendMessage:
                            {
                                AService service = this.Get(op.ServiceId);
                                if (service != null)
                                {
                                    service.Send(op.ChannelId, op.Object);
                                }
                                break;
                            }
                        default:
                            throw new Exception($"not found net operator: {op.Op}");
                    }
                }
                catch (Exception e)
                {
                    LogUtil.Error(e);
                }
            }
        }

        public void UpdateInNetThread()
        {
            int count = this.queue.Count;
            while (count-- > 0)
            {
                int serviceId = this.queue.Dequeue();
                if (!this.services.TryGetValue(serviceId, out AService service))
                {
                    continue;
                }
                this.queue.Enqueue(serviceId);
                service.Update();
            }

            this.RunNetThreadOperator();
        }

        public void OnRead(int serviceId, long channelId, object message)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnRead, ServiceId = serviceId, ChannelId = channelId, Object = message };
            this.mainThreadOperators.Enqueue(netOperator);
        }

        public void OnError(int serviceId, long channelId, int error)
        {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnError, ServiceId = serviceId, ChannelId = channelId, Object = error };
            this.mainThreadOperators.Enqueue(netOperator);
        }

        #endregion

        #region 主线程kcp id生成

        // 这个因为是NetClientComponent中使用，不会与Accept冲突
        public uint CreateConnectChannelId()
        {
            return 1;// RandomGenerator.RandUInt32();
        }

        #endregion

    }
}