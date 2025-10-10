using System.Collections.Generic;
using Steamworks;
using UnityEngine;

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
            Main.FishNetService.SetNetworkAddress();
            Main.FishNetService.StartServer();
        }

        /// <summary>
        /// 大厅进入完成
        /// </summary>
        /// <param name="data"></param>
        public void OnLobbyEnter(LobbyData lobbyData)
        {
            string hostAddress = lobbyData.GetLobbyCustomData("HostAddress");
            Main.FishNetService.SetNetworkAddress(hostAddress);
            Main.FishNetService.Connect();
        }

        /// <summary>
        /// 离开大厅完成
        /// </summary>
        public void OnLeaveLobby()
        {
            if (Main.FishNetService.IsNetworkActive)
            {
                Main.FishNetService.Disconnect();
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