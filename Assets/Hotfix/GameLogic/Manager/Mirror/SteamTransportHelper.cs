using Mirror.FizzySteam;
using UnityEngine;

namespace LccHotfix
{
    public class SteamTransportHelper : IMirrorTransportHelper
    {
        public void SetupTransport(GameObject networkManager)
        {
            var transport = networkManager.AddComponent<FizzySteamworks>();
        }
    }
}