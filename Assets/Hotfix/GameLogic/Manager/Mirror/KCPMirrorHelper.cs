using kcp2k;
using UnityEngine;

namespace LccHotfix
{
    public class KCPMirrorHelper : IMirrorHelper
    {
        public MirrorNetworkManager Setup()
        {
            GameObject obj = new GameObject();
            obj.SetActive(false);
            obj.name = "MirrorNetworkManagerKCP";
            GameObject.DontDestroyOnLoad(obj);
            var networkManager = obj.AddComponent<MirrorNetworkManager>();
            var transport = obj.AddComponent<KcpTransport>();
            transport.Port = 7788;
            networkManager.transport = transport;
            obj.SetActive(true);
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