using Mirror.FizzySteam;
using UnityEngine;

namespace LccHotfix
{
    public class SteamTransportHelper : ITransportHelper
    {
        public void SetupTransport(GameObject networkManager)
        {
            var transport = networkManager.AddComponent<FizzySteamworks>();
        }
    }
}