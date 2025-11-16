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
                var obj = x.AssetObject as GameObject;
                GameObject.DontDestroyOnLoad(obj);
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