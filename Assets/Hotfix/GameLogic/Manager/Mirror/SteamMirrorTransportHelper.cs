using Mirror.FizzySteam;
using UnityEngine;

namespace LccHotfix
{
    public class SteamMirrorTransportHelper : IMirrorTransportHelper
    {
        public void SetupTransport(GameObject networkManager)
        {
            var transport = networkManager.AddComponent<FizzySteamworks>();
        }
    }
}