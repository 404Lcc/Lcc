using UnityEngine;

namespace LccHotfix
{
    public static class NetworkUtility
    {
        public static bool CheckNetwork()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //无网络
                return false;
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                //流量
                return true;
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                //wifi
                return true;
            }
            return false;
        }
    }
}