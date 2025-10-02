using System;
using System.Collections.Generic;
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
    Join,
    Leave
}

public class SteamLobbyManager : MonoBehaviour
{
    [Serializable] public class LobbyStatusEvent : UnityEvent<LobbyStatus> { }
    [Serializable] public class JoinStatusEvent : UnityEvent<JoinStatus> { }
    [Serializable] public class LobbyMemberEvent : UnityEvent<int> { }
    public CSteamID HostId { get; private set; }
    public bool isHost;
    public LobbyStatus lobbyStatus { get; private set; } =  LobbyStatus.None;
    public JoinStatus joinStatus { get; private set; } = JoinStatus.None;

    public LobbyStatusEvent lobbyStatusEvent = new LobbyStatusEvent();
    public JoinStatusEvent joinStatusEvent = new JoinStatusEvent();

    [Serializable]
    public struct LobbyMemberInfo
    {
        public CSteamID id;
        public string name;
        public bool isHost;
        public Texture2D avatar;
    }
    
    public UnityEvent<LobbyMemberInfo> OnJoinLobby = new UnityEvent<LobbyMemberInfo>();
    public UnityEvent<LobbyMemberInfo> OnLeaveLobby = new UnityEvent<LobbyMemberInfo>();
    public UnityEvent<List<LobbyMemberInfo>> OnClientJoinLobby = new UnityEvent<List<LobbyMemberInfo>>();
    
    public static SteamLobbyManager Instance { get; private set; }

    public CSteamID LobbyId { get; private set; }
    private string lobbyName;
    
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


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!SteamManager.Instance.Initialized) return;
        
        // 콜백 등록
        lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyJoinRequestCallback = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequest);
        lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    public bool IsHost(CSteamID steamId)
    {
        return HostId == steamId;
    }
    
    // Invite Friend
    public bool InviteFriend()
    {
        if (!LobbyId.IsValid()) return false;

        if (!SteamUtils.IsOverlayEnabled())
        {
            Debug.Log("overlay is disabled");
            return false;
        }

        Debug.Log("InviteFriend");
        SteamFriends.ActivateGameOverlayInviteDialog(LobbyId);
        return true;
    }

    public void CreateLobby(ELobbyType lobbyType, int maxMembers, string lobbyNameData)
    {
        SteamMatchmaking.CreateLobby(lobbyType, maxMembers);
        lobbyName = lobbyNameData;
    }

    public LobbyMemberInfo GetLobbyMember(CSteamID steamId)
    {
        return new LobbyMemberInfo()
        {
            id = steamId,
            name = SteamFriends.GetFriendPersonaName(steamId),
            isHost = IsHost(steamId),
            avatar = GetSteamAvatar(steamId)
        };
    }

    public List<LobbyMemberInfo> GetLobbyMembers(CSteamID lobbyId)
    {
        List<LobbyMemberInfo> members = new List<LobbyMemberInfo>();
        
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobbyId);

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
            LobbyMemberInfo memberInfo = GetLobbyMember(memberId);
            members.Add(memberInfo);
        }
        
        return members;
    }
    public Texture2D GetSteamAvatar(CSteamID steamId)
    {
        int avatarHandle = SteamFriends.GetMediumFriendAvatar(steamId); // 64x64
    
        if (avatarHandle == -1)
        {
            Debug.Log("아바타 로딩 중...");
            return null; // 아직 로드 안됨
        }
    
        if (avatarHandle == 0)
        {
            Debug.LogError("아바타 없음");
            return null;
        }
    
        // 아바타 크기 확인
        uint width, height;
        SteamUtils.GetImageSize(avatarHandle, out width, out height);
    
        // 이미지 데이터 가져오기
        byte[] imageData = new byte[width * height * 4]; // RGBA
        SteamUtils.GetImageRGBA(avatarHandle, imageData, (int)(width * height * 4));
    
        // Texture2D로 변환
        Texture2D avatarTexture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
        avatarTexture.LoadRawTextureData(imageData);
        avatarTexture.Apply();
    
        return avatarTexture;
    }
    
    // host가 lobby 생성시 콜백
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            HostId = SteamUser.GetSteamID();
        
            Debug.Log("Successfully created lobby");

            // 로비 데이터 설정
            SteamMatchmaking.SetLobbyData(LobbyId, "name", lobbyName);
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
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    // lobby enter시 콜백 (client)
    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        
        if (callback.m_EChatRoomEnterResponse == 1)
        {
            HostId = SteamMatchmaking.GetLobbyOwner(LobbyId);

            Debug.Log("로비 입장 성공!");
            SetJoinStatus(JoinStatus.Join);
            
            List<LobbyMemberInfo> playerInfo = GetLobbyMembers(LobbyId);
            
            OnClientJoinLobby?.Invoke(playerInfo);
            // SteamNetworking.CloseP2PSessionWithUser(HostId);
            // Debug.Log("P2P 세션 강제 종료");
        }
        else
        {
            SetJoinStatus(JoinStatus.FailedToJoin);
            Debug.LogError("로비 입장 실패!");
        }
    }
    
    // Todo: when join or leave, invoke join or leave message
    // SteamMatchmaking.SendLobbyChatMsg(lobbyId, Encoding.UTF8.GetBytes(message), message.Length);

    // 로비에 있을 때 누군가 로비에서 업데이트 될 때 콜백
    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        CSteamID userId = new CSteamID(callback.m_ulSteamIDUserChanged);
        uint stateChange = callback.m_rgfChatMemberStateChange;
        
        LobbyMemberInfo playerInfo = GetLobbyMember(userId);
        
        // 입장 체크
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered) != 0)
        {
            Debug.Log("플레이어 입장!");
            OnJoinLobby?.Invoke(playerInfo);
            // 새 플레이어 처리
        }
        else
        {
            OnLeaveLobby?.Invoke(playerInfo);
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
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(LobbyId);
        int maxMembers = SteamMatchmaking.GetLobbyMemberLimit(LobbyId);
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