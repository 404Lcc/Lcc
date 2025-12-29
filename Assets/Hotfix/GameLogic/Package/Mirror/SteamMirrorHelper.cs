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
                GameObject root = new GameObject("MirrorRoot");
                var obj = GameObject.Instantiate(x.AssetObject as GameObject);
                obj.transform.SetParent(root.transform);
                GameObject.DontDestroyOnLoad(root);
                
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