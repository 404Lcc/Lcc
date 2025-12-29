using System;
using FishNet.Managing.Transporting;
using UnityEngine;

namespace LccHotfix
{
    public class SteamFishNetHelper : IFishNetHelper
    {
        public void Setup(AssetLoader loader, Action<FishNet.Managing.NetworkManager> onFinished)
        {
            loader.LoadAssetAsync<GameObject>("FishNetManagerSteam", (x) =>
            {
                GameObject root = new GameObject("FishNetRoot");
                var obj = GameObject.Instantiate(x.AssetObject as GameObject);
                obj.transform.SetParent(root.transform);
                GameObject.DontDestroyOnLoad(root);
                
                var networkManager = obj.GetComponent<FishNet.Managing.NetworkManager>();
                onFinished(networkManager);
            });
        }

        public void SetNetworkAddress(GameObject networkManager, string networkAddress)
        {
            networkManager.GetComponent<TransportManager>().Transport.SetClientAddress(networkAddress);
        }
    }
}