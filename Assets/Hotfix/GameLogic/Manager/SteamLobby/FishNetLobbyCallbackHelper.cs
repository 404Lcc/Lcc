using System.Collections.Generic;
using Steamworks;

namespace LccHotfix
{
    public class FishNetLobbyCallbackHelper : ILobbyCallbackHelper
    {
        /// <summary>
        /// 大厅创建完成
        /// </summary>
        /// <param name="data"></param>
        public void OnLobbyCreated(LobbyData lobbyData)
        {
            lobbyData.SetLobbyCustomData("HostAddress", SteamUser.GetSteamID().ToString());
        }

        /// <summary>
        /// 大厅进入完成
        /// </summary>
        /// <param name="data"></param>
        public void OnLobbyEnter(LobbyData lobbyData)
        {
            string hostAddress = lobbyData.GetLobbyCustomData("HostAddress");
            Main.FishNetService.Register();
            Main.FishNetService.SetNetworkAddress(hostAddress);
            // 自己是房主
            if (SteamUser.GetSteamID().m_SteamID.ToString() == hostAddress)
            {
                Main.FishNetService.StartServer();
            }

            Main.FishNetService.Connect();
        }

        /// <summary>
        /// 离开大厅完成
        /// </summary>
        public void OnLeaveLobby(LobbyData lobbyData)
        {
            string hostAddress = lobbyData.GetLobbyCustomData("HostAddress");
            Main.FishNetService.Unregister();
            Main.FishNetService.Disconnect();
            // 自己是房主
            if (SteamUser.GetSteamID().m_SteamID.ToString() == hostAddress)
            {
                Main.FishNetService.StopServer();
            }
        }

        /// <summary>
        /// 大厅列表查找完成
        /// </summary>
        /// <param name="dataList"></param>
        public void OnLobbyMatchListCallback(List<LobbyData> lobbyDataList)
        {

        }
    }
}