using FishNet.Managing.Transporting;
using FishNet.Transporting;
using UnityEngine;

namespace LccHotfix
{
    public class SteamFishNetHelper : IFishNetHelper
    {
        public FishNet.Managing.NetworkManager Setup()
        {
            Main.AssetService.LoadGameObject("FishNetManagerSteam", true, out var res);
            GameObject.DontDestroyOnLoad(res);
            var networkManager = res.GetComponent<FishNet.Managing.NetworkManager>();
            return networkManager;
        }

        public void SetNetworkAddress(GameObject networkManager, string networkAddress)
        {
            networkManager.GetComponent<TransportManager>().Transport.SetClientAddress(networkAddress);
        }
    }
}