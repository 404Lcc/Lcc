using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public interface ISteamLobbyService : IService
    {
        LobbyData CurrentLobbyData { get; set; }

        // 创建大厅
        void CreateLobby(Action<LobbyData> action);

        // 加入大厅
        void JoinLobby(ulong lobbyID, Action<LobbyData> action);

        // 离开大厅
        void LeaveLobby();

        // 查找大厅列表
        void RequestLobbyList(int maxResults, Action<List<LobbyData>> action);

        // 获取大厅数据
        LobbyData GetLobbyData(ulong lobbyID);
    }
}