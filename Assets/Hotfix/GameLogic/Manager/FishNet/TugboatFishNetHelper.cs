using FishNet.Managing.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace LccHotfix
{
    public class TugboatFishNetHelper : IFishNetHelper
    {
        public FishNet.Managing.NetworkManager Setup()
        {
            Main.AssetService.LoadGameObject("FishNetManagerTugboat", true, out var res);
            GameObject.DontDestroyOnLoad(res);
            var networkManager = res.GetComponent<FishNet.Managing.NetworkManager>();
            var transport = networkManager.GetComponent<Tugboat>();
            transport.SetPort(7788);
            return networkManager;
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