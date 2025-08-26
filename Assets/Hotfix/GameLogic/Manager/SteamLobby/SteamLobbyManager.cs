using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace LccHotfix
{
    public class LobbyData
{
    private ulong _lobbyID;
    public CSteamID LobbyID { get; set; }

    public void InitData(ulong lobbyID)
    {
        this._lobbyID = lobbyID;
        LobbyID = new CSteamID(lobbyID);
    }

    /// <summary>
    /// 获取大厅名称
    /// </summary>
    /// <returns></returns>
    public string GetLobbyName()
    {
        return SteamMatchmaking.GetLobbyData(LobbyID, "name");
    }

    /// <summary>
    /// 获取大厅自定义数据
    /// </summary>
    /// <returns></returns>
    public string GetLobbyCustomData()
    {
        return SteamMatchmaking.GetLobbyData(LobbyID, "custom_key");
    }

    /// <summary>
    /// 获取大厅内成员数量
    /// </summary>
    /// <returns></returns>
    public int GetLobbyMemberCount()
    {
        return SteamMatchmaking.GetNumLobbyMembers(LobbyID);
    }

    /// <summary>
    /// 获取所有成员
    /// </summary>
    /// <returns></returns>
    public List<CSteamID> GetLobbyMemberList()
    {
        List<CSteamID> list = new List<CSteamID>();
        for (int i = 0; i < GetLobbyMemberCount(); i++)
        {
            list.Add(SteamMatchmaking.GetLobbyMemberByIndex(LobbyID, i));
        }

        return list;
    }

    /// <summary>
    /// 获取成员列表的名字
    /// </summary>
    /// <returns></returns>
    public List<string> GetLobbyMemberNameList()
    {
        List<string> list = new List<string>();
        foreach (var item in GetLobbyMemberList())
        {
            list.Add(SteamFriends.GetFriendPersonaName(item));
        }

        return list;
    }
}

public class SteamLobbyManager : Module, ISteamLobbyService
{
    // 创建大厅的回调
    private Callback<LobbyCreated_t> _lobbyCreatedCallResult;

    // 加入大厅的回调
    private Callback<LobbyEnter_t> _lobbyEnterCallResult;

    // 查看大厅列表的回调
    private Callback<LobbyMatchList_t> _lobbyMatchListCallResult;

    // 大厅创建完成回调
    private Action<LobbyData> _lobbyCreatedAction;

    // 大厅进入完成回调
    private Action<LobbyData> _lobbyEnterAction;

    // 大厅列表查找完成的回调
    private Action<List<LobbyData>> _lobbyMatchListAction;

    public LobbyData CurrentLobbyData { get; set; }


    public SteamLobbyManager()
    {
        _lobbyCreatedCallResult = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _lobbyEnterCallResult = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        _lobbyMatchListCallResult = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
    }

    internal override void Update(float elapseSeconds, float realElapseSeconds)
    {
    }

    internal override void Shutdown()
    {
        CurrentLobbyData = null;
    }


    // 创建大厅
    public void CreateLobby(Action<LobbyData> action)
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic /* 或者你想用的其他类型 */, 4 /* 最大玩家数 */);
        this._lobbyCreatedAction = action;
    }

    // 加入大厅
    public void JoinLobby(ulong lobbyID, Action<LobbyData> action)
    {
        CSteamID id = new CSteamID(lobbyID);
        SteamMatchmaking.JoinLobby(id);
        this._lobbyEnterAction = action;
    }

    // 离开大厅
    public void LeaveLobby()
    {
        if (CurrentLobbyData == null)
            return;

        SteamMatchmaking.LeaveLobby(CurrentLobbyData.LobbyID);
    }

    // 查找大厅列表
    public void RequestLobbyList(int maxResults, Action<List<LobbyData>> action)
    {
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(maxResults); // 查找30个大厅
        SteamMatchmaking.RequestLobbyList();
        this._lobbyMatchListAction = action;
    }

    // 获取大厅数据
    public LobbyData GetLobbyData(ulong lobbyID)
    {
        CSteamID id = new CSteamID(lobbyID);
        if (id.IsValid())
        {
            LobbyData data = new LobbyData();
            data.InitData(lobbyID);
            return data;
        }

        return null;
    }

    /// <summary>
    /// 创建大厅的回调
    /// </summary>
    /// <param name="param"></param>
    private void OnLobbyCreated(LobbyCreated_t param)
    {
        if (param.m_eResult == EResult.k_EResultOK)
        {
            LobbyData data = new LobbyData();
            data.InitData(param.m_ulSteamIDLobby);

            var name = SteamFriends.GetPersonaName();
            // 设置大厅名称
            SteamMatchmaking.SetLobbyData(data.LobbyID, "name", name);

            CurrentLobbyData = data;

            _lobbyCreatedAction?.Invoke(CurrentLobbyData);
        }
        else
        {
            Debug.LogError("创建大厅失败" + param.m_eResult);
        }
    }

    /// <summary>
    /// 加入大厅的回调
    /// </summary>
    /// <param name="param"></param>
    private void OnLobbyEnter(LobbyEnter_t param)
    {
        if (param.m_EChatRoomEnterResponse == (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            CurrentLobbyData = GetLobbyData(param.m_ulSteamIDLobby);

            _lobbyEnterAction?.Invoke(CurrentLobbyData);
        }
        else
        {
            Debug.LogError("加入大厅失败" + param.m_EChatRoomEnterResponse);
        }
    }

    /// <summary>
    /// 查看大厅列表的回调
    /// </summary>
    /// <param name="param"></param>
    private void OnLobbyMatchList(LobbyMatchList_t param)
    {
        var list = new List<LobbyData>();
        for (int i = 0; i < param.m_nLobbiesMatching; i++)
        {
            var data = GetLobbyData(SteamMatchmaking.GetLobbyByIndex(i).m_SteamID);
            list.Add(data);
        }

        _lobbyMatchListAction?.Invoke(list);
    }
}
}