using kcp2k;
using UnityEngine;

namespace LccHotfix
{
    public class KCPTransportHelper : ITransportHelper
    {
        public void SetupTransport(GameObject networkManager)
        {
            var transport = networkManager.AddComponent<KcpTransport>();
            transport.Port = 7788;
        }
    }
}