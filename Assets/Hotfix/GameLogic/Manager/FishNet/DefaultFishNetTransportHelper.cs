using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace LccHotfix
{
    public class DefaultFishNetTransportHelper : IFishNetTransportHelper
    {
        public void SetupTransport(GameObject networkManager)
        {
            var transport = networkManager.GetComponent<Tugboat>();
            transport.SetPort(7788);
        }
    }
}