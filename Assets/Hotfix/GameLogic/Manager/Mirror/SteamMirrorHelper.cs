using System;
using Mirror.FizzySteam;
using UnityEngine;

namespace LccHotfix
{
    public class SteamMirrorHelper : IMirrorHelper
    {
        public void Setup(AssetLoader loader, Action<MirrorNetworkManager> onFinished)
        {
            loader.LoadAssetAsync<GameObject>("MirrorNetworkManagerSteam", (x) =>
            {
                var obj = x.AssetObject as GameObject;
                GameObject.DontDestroyOnLoad(obj);
                var networkManager = obj.GetComponent<MirrorNetworkManager>();
                onFinished(networkManager);
            });

        }

        public void SetNetworkAddress(GameObject networkManager, string networkAddress)
        {
            networkManager.GetComponent<MirrorNetworkManager>().networkAddress = networkAddress;
        }
    }
}