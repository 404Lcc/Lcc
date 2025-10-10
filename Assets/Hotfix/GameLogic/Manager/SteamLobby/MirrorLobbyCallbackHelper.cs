using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;

namespace LccHotfix
{
    public class MirrorLobbyCallbackHelper : ILobbyCallbackHelper
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
            Main.MirrorService.Register();
            Main.MirrorService.SetNetworkAddress(hostAddress);
            // 自己是房主
            if (SteamUser.GetSteamID().m_SteamID.ToString() == hostAddress)
            {
                Main.MirrorService.StartServer();
            }
            else
            {
                Main.MirrorService.Connect();
            }
        }

        /// <summary>
        /// 离开大厅完成
        /// </summary>
        public void OnLeaveLobby(LobbyData lobbyData)
        {
            string hostAddress = lobbyData.GetLobbyCustomData("HostAddress");
            Main.MirrorService.Unregister();
            // 自己是房主
            if (SteamUser.GetSteamID().m_SteamID.ToString() == hostAddress)
            {
                Main.MirrorService.StopServer();
            }
            else
            {
                Main.MirrorService.Disconnect();
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