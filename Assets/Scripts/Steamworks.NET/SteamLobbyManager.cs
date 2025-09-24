using Steamworks;
using UnityEngine;
using UnityEngine.Events;

public class SteamLobbyManager : MonoBehaviour
{
    [System.Serializable]
    public class LobbyEvent : UnityEvent<LobbyStatus, string> { } // 성공여부, 메시지

   
    public enum LobbyStatus
    {
        CREATING,
        CREATED,
        FAILED,
    }
    public LobbyEvent lobbyEvent = new LobbyEvent();
    
    private CSteamID lobbyId;
    private bool isHost = false;
    
    public CSteamID LobbyId => lobbyId;
    
    // 콜백 핸들러들
    private Callback<LobbyCreated_t> lobbyCreatedCallback;
    private Callback<GameLobbyJoinRequested_t> lobbyJoinRequestCallback;
    private Callback<LobbyEnter_t> lobbyEnterCallback;
    
    void Start()
    {
        if (!SteamManager.Instance.IsInitialized) return;
        
        // 콜백 등록
        lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyJoinRequestCallback = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequest);
        lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }
    
    public void CreateLobby(ELobbyType lobbyType, int maxMembers)
    {
        if (!SteamManager.Instance.IsInitialized)
        {
            lobbyEvent?.Invoke(LobbyStatus.FAILED, "Failed to create lobby");
            return;
        }
        
        lobbyEvent?.Invoke(LobbyStatus.CREATING, "Creating lobby");
        Debug.Log("Creating lobby..");
        SteamMatchmaking.CreateLobby(lobbyType, maxMembers);
    }
    
    public void InviteFriend()
    {
        if (lobbyId.IsValid())
        {
            SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);
        }
    }
    
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            isHost = true;
        
            Debug.Log("Successfully created lobby");
        
            // 로비 데이터 설정
            SteamMatchmaking.SetLobbyData(lobbyId, "name", "Game");
            SteamMatchmaking.SetLobbyData(lobbyId, "version", "1.0");
            
            lobbyEvent?.Invoke(LobbyStatus.CREATED, "Successfully created lobby");
        }
        else
        {
            lobbyEvent?.Invoke(LobbyStatus.FAILED, $"failed to create lobby: {callback.m_eResult}");
            Debug.LogError("로비 생성 실패!");
            return;
        }
    }

    private void OnLobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("로비 참가 요청 받음");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        
        if (callback.m_EChatRoomEnterResponse == 1)
        {
            Debug.Log("로비 입장 성공!");
            
            // 로비의 다른 멤버 확인
            int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
            Debug.Log($"로비 멤버 수: {numMembers}");
            
            if (numMembers == 2)
            {
                StartGame();
            }
        }
        else
        {
            Debug.LogError("로비 입장 실패!");
        }
    }
    
    void StartGame()
    {
        Debug.Log("게임 시작!");
        // P2P 연결 시작
        GetComponent<SteamP2PManager>().StartP2PConnection();
    }
}