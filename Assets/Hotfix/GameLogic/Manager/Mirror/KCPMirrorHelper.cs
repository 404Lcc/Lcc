using System;
using kcp2k;
using UnityEngine;

namespace LccHotfix
{
    public class KCPMirrorHelper : IMirrorHelper
    {
        public void Setup(AssetLoader loader, Action<MirrorNetworkManager> onFinished)
        {
            loader.LoadAssetAsync<GameObject>("MirrorNetworkManagerSteam", (x) =>
            {
                var obj = x.AssetObject as GameObject;
                GameObject.DontDestroyOnLoad(obj);
                var networkManager = obj.GetComponent<MirrorNetworkManager>();
                var transport = networkManager.GetComponent<KcpTransport>();
                transport.Port = 7788;
                onFinished(networkManager);
            });

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