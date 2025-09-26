using System.Collections;
using System.Collections.Generic;
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
            Main.MirrorService.SetNetworkAddress();
            Main.MirrorService.StartHost();
        }

        /// <summary>
        /// 大厅进入完成
        /// </summary>
        /// <param name="data"></param>
        public void OnLobbyEnter(LobbyData lobbyData)
        {
            string hostAddress = lobbyData.GetLobbyCustomData("HostAddress");
            Main.MirrorService.SetNetworkAddress(hostAddress);
            Main.MirrorService.Connect();
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