using kcp2k;
using UnityEngine;

namespace LccHotfix
{
    public class KCPMirrorTransportHelper : IMirrorHelper
    {
        public MirrorNetworkManager Setup()
        {
            Main.AssetService.LoadGameObject("MirrorNetworkManagerKCP", true, out var res);
            GameObject.DontDestroyOnLoad(res);
            var networkManager = res.GetComponent<MirrorNetworkManager>();
            var transport = networkManager.GetComponent<KcpTransport>();
            transport.Port = 7788;
            return networkManager;
        }

        public void SetNetworkAddress(GameObject networkManager, string networkAddress)
        {
            if (string.IsNullOrEmpty(networkAddress))
            {
                networkAddress = GameUtility.GetLocalIPAddress();
            }

            networkManager.GetComponent<MirrorNetworkManager>().networkAddress = networkAddress;
        }
    }
}