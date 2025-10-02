using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.Events;

public class LobbyListManager : MonoBehaviour
{
    [Serializable]
    public struct LobbyInfo
    {
        public CSteamID lobbyID;
        public string lobbyName;
        public int currentMembers;
        public int maxMembers;
    }
    
    private Callback<LobbyMatchList_t> lobbyListCallback;
    private List<LobbyInfo> lobbyList = new List<LobbyInfo>();
    
    
    private void Start()
    {
        lobbyListCallback = Callback<LobbyMatchList_t>.Create(OnLobbyListReceived);
    }
    
    public UnityEvent<List<LobbyInfo>> OnLobbyListUpdated = new UnityEvent<List<LobbyInfo>>();

    public void RequestLobbyList()
    {
        Debug.Log("RequestLobbyList");
        
        // 필터 설정 (선택사항)
        // SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
        // SteamMatchmaking.AddRequestLobbyListResultCountFilter(50); // 최대 50개
        
        // 로비 검색 시작
        SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyListReceived(LobbyMatchList_t callback)
    {
        Debug.Log($"{callback.m_nLobbiesMatching} lobbies found");
        
        lobbyList.Clear();

        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            LobbyInfo info = GetLobbyInfo(lobbyID);
            lobbyList.Add(info);
        }
        
        OnLobbyListUpdated?.Invoke(lobbyList);
    }

    private LobbyInfo GetLobbyInfo(CSteamID lobbyID)
    {
        return new LobbyInfo
        {
            lobbyID = lobbyID,
            lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, "name"),
            currentMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyID),
            maxMembers = SteamMatchmaking.GetLobbyMemberLimit(lobbyID)
        };
    }
}
