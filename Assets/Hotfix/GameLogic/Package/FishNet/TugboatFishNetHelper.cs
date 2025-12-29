using System;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace LccHotfix
{
    public class TugboatFishNetHelper : IFishNetHelper
    {
        public void Setup(AssetLoader loader, Action<FishNet.Managing.NetworkManager> onFinished)
        {
            loader.LoadAssetAsync<GameObject>("FishNetManagerTugboat", (x) =>
            {
                GameObject root = new GameObject("FishNetRoot");
                var obj = GameObject.Instantiate(x.AssetObject as GameObject);
                obj.transform.SetParent(root.transform);
                GameObject.DontDestroyOnLoad(root);
                
                var networkManager = obj.GetComponent<FishNet.Managing.NetworkManager>();
                var transport = networkManager.GetComponent<Tugboat>();
                transport.SetPort(7788);
            });
        }

        public void SetNetworkAddress(GameObject networkManager, string networkAddress)
        {
            if (string.IsNullOrEmpty(networkAddress))
            {
                networkAddress = GameUtility.GetLocalIPAddress();
            }

            networkManager.GetComponent<TransportManager>().Transport.SetClientAddress(networkAddress);
        }
    }
}