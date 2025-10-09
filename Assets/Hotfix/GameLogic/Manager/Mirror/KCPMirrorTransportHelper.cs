using kcp2k;
using UnityEngine;

namespace LccHotfix
{
    public class KCPMirrorTransportHelper : IMirrorTransportHelper
    {
        public void SetupTransport(GameObject networkManager)
        {
            var transport = networkManager.AddComponent<KcpTransport>();
            transport.Port = 7788;
        }
    }
}