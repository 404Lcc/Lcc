using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace LccHotfix
{
    public interface ILobbyCallbackHelper
    {
        void OnLobbyCreated(LobbyData lobbyData);

        void OnLobbyEnter(LobbyData lobbyData);

        void OnLobbyLeave(LobbyData lobbyData);

        void OnLobbyMemberStateChange(EChatMemberStateChange type);

        void OnLobbyMatchListCallback(List<LobbyData> lobbyDataList);
    }
}