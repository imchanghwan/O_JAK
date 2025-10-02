using System;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

public enum LobbyStatus
{
    None,
    Creating,
    Created,
    FailedToCreate,
    Starting,
    Started,
    FailedToStart,
    Playing,
    Disconnected,
    Error,
    SteamError
}
public enum JoinStatus
{
    None,
    FailedToJoin,
    Joining,
    Joined,
}

public class SteamLobbyManager : MonoBehaviour
{
    [Serializable] public class LobbyStatusEvent : UnityEvent<LobbyStatus> { }
    [Serializable] public class JoinStatusEvent : UnityEvent<JoinStatus> { }
    [Serializable] public class LobbyMemberEvent : UnityEvent<int> { }
    public CSteamID HostId { get; private set; }
    public bool IsHost => HostId == SteamUser.GetSteamID();
    public LobbyStatus lobbyStatus { get; private set; } =  LobbyStatus.None;
    public JoinStatus joinStatus { get; private set; } = JoinStatus.None;

    public LobbyStatusEvent lobbyStatusEvent = new LobbyStatusEvent();
    public JoinStatusEvent joinStatusEvent = new JoinStatusEvent();

    public static CSteamID lobbyId { get; private set; }
    private void SetLobbyStatus(LobbyStatus newStatus)
    {
        if (lobbyStatus == newStatus) return;
        LobbyStatus oldStatus = lobbyStatus;
        lobbyStatus = newStatus;
        lobbyStatusEvent?.Invoke(newStatus);
        Debug.Log($"로비 상태 변경: {oldStatus} → {newStatus}");
    }
    private void SetJoinStatus(JoinStatus newStatus)
    {
        if (joinStatus == newStatus) return;
        JoinStatus oldStatus = joinStatus;
        joinStatus = newStatus;
        joinStatusEvent?.Invoke(newStatus);
        Debug.Log($"로비 상태 변경: {oldStatus} → {newStatus}");
    }
    
    // 콜백 핸들러들
    private Callback<LobbyCreated_t> lobbyCreatedCallback;
    private Callback<GameLobbyJoinRequested_t> lobbyJoinRequestCallback;
    private Callback<LobbyEnter_t> lobbyEnterCallback;
    private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback;
    
    void Start()
    {
        if (!SteamManager.Instance.Initialized) return;
        
        // 콜백 등록
        lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyJoinRequestCallback = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequest);
        lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }
    
    // Create Lobby
    public void CreateLobby(ELobbyType lobbyType, int maxMembers)
    {
        if (!SteamManager.Instance.Initialized)
        {
            SetLobbyStatus(LobbyStatus.SteamError);
            return;
        }

        SetLobbyStatus(LobbyStatus.Creating);
        Debug.Log("Creating lobby..");
        SteamMatchmaking.CreateLobby(lobbyType, maxMembers);
    }
    
    // Invite Friend
    public void InviteFriend()
    {
        if (!lobbyId.IsValid()) return;
        if (SteamUtils.IsOverlayEnabled())
        {
            Debug.Log("InviteFriend");
            SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);
        }
        else
        {
            Debug.Log("overlay is disabled");
        }
    }

    // Join Lobby By Id (!host용)
    public void JoinLobbyById(CSteamID id)
    {
        if (IsHost) return;
        SetJoinStatus(JoinStatus.Joining);
        SteamMatchmaking.JoinLobby(id);
    }

    // Leave Lobby
    public void LeaveLobby()
    {
        if (!lobbyId.IsValid()) return;
        SetLobbyStatus(LobbyStatus.None);
        SetJoinStatus(JoinStatus.None);
        SteamMatchmaking.LeaveLobby(lobbyId);
    }
    
    // lobby 생성시 콜백
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            HostId = SteamUser.GetSteamID();
        
            Debug.Log("Successfully created lobby");
        
            // 로비 데이터 설정
            SteamMatchmaking.SetLobbyData(lobbyId, "name", "Game");
            SteamMatchmaking.SetLobbyData(lobbyId, "version", "1.0");
            SetLobbyStatus(LobbyStatus.Created);
        }
        else
        {
            SetLobbyStatus(LobbyStatus.FailedToCreate);
            Debug.LogError("로비 생성 실패!");
        }
    }

    // lobby join request 외부(초대링크, 게임참가 등)에서 join할시 콜백 (!host용)
    private void OnLobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SetJoinStatus(JoinStatus.Joining);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    // lobby enter시 콜백 (both host and !host)
    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        
        if (callback.m_EChatRoomEnterResponse == 1)
        {
            HostId = SteamMatchmaking.GetLobbyOwner(lobbyId);

            Debug.Log("로비 입장 성공!");
            SetJoinStatus(JoinStatus.Joined);
            SteamNetworking.CloseP2PSessionWithUser(HostId);
            Debug.Log("P2P 세션 강제 종료");
        }
        else
        {
            SetJoinStatus(JoinStatus.FailedToJoin);
            Debug.LogError("로비 입장 실패!");
        }
    }
    
    // Todo: when join or leave, invoke join or leave message
    // SteamMatchmaking.SendLobbyChatMsg(lobbyId, Encoding.UTF8.GetBytes(message), message.Length);

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        CSteamID userId = new CSteamID(callback.m_ulSteamIDUserChanged);
        uint stateChange = callback.m_rgfChatMemberStateChange;
        // 입장시 member data 자신제외 보내기
        // 퇴장시 member 제거
        
        // 입장 체크
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered) != 0)
        {
            Debug.Log("플레이어 입장!");
            // 새 플레이어 처리
        }
    
        // 퇴장 체크  
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft) != 0)
        {
            Debug.Log("플레이어 퇴장!");
        }
    
        // 연결 끊김
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) != 0)
        {
            Debug.Log("플레이어 연결 끊김!");
        }
    
        // 강제 퇴장
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeKicked) != 0)
        {
            Debug.Log("플레이어 강제 퇴장!");
        }
    }

    public void StartGame()
    {
        // TODO:
        // 로비 멤버 모이지 않으면 return;
        // 각 플레이어에게 sendMessage로 세션 연결
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        int maxMembers = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);
        if (numMembers == maxMembers)
        {
            SteamP2PManager.NetworkMessage message = new SteamP2PManager.NetworkMessage
            {
                messageType = SteamP2PManager.MessageType.Message,
                data = "start"
            };
            GetComponent<SteamP2PManager>().HostSendMessage(message);
            Debug.Log("start game");
        }
    }
}