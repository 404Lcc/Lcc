using UnityEngine;

namespace LccHotfix
{
    public class SteamMirrorTransportHelper : IMirrorHelper
    {
        public MirrorNetworkManager Setup()
        {
            Main.AssetService.LoadGameObject("MirrorNetworkManagerSteam", true, out var res);
            GameObject.DontDestroyOnLoad(res);
            var networkManager = res.GetComponent<MirrorNetworkManager>();
            return networkManager;
        }

        public void SetNetworkAddress(GameObject networkManager, string networkAddress)
        {
            networkManager.GetComponent<MirrorNetworkManager>().networkAddress = networkAddress;
        }
    }
}