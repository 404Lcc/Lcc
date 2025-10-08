using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface ISteamLobbyService : IService
    {
        LobbyData CurrentLobbyData { get; set; }

        void SetLobbyCallbackHelper(ILobbyCallbackHelper lobbyCallbackHelper);

        // 创建大厅
        void CreateLobby();

        // 加入大厅
        void JoinLobby(ulong lobbyID);

        // 邀请加入大厅
        void InviteJoinLobby();

        // 离开大厅
        void LeaveLobby();

        // 查找大厅列表
        void RequestLobbyList(int maxResults);

        // 获取大厅数据
        LobbyData GetLobbyData(ulong lobbyID);
    }
}