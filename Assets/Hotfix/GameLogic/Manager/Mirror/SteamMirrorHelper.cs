using Mirror.FizzySteam;
using UnityEngine;

namespace LccHotfix
{
    public class SteamMirrorHelper : IMirrorHelper
    {
        public MirrorNetworkManager Setup()
        {
            GameObject obj = new GameObject();
            obj.SetActive(false);
            obj.name = "MirrorNetworkManagerSteam";
            GameObject.DontDestroyOnLoad(obj);
            var networkManager = obj.AddComponent<MirrorNetworkManager>();
            var transport = obj.AddComponent<FizzySteamworks>();
            networkManager.transport = transport;
            obj.SetActive(true);
            return networkManager;
        }

        public void SetNetworkAddress(GameObject networkManager, string networkAddress)
        {
            networkManager.GetComponent<MirrorNetworkManager>().networkAddress = networkAddress;
        }
    }
}