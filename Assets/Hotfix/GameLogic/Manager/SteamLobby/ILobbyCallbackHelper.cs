using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface ILobbyCallbackHelper
    {
        void OnLobbyCreated(LobbyData lobbyData);

        void OnLobbyEnter(LobbyData lobbyData);

        void OnLobbyMatchListCallback(List<LobbyData> lobbyDataList);
    }
}