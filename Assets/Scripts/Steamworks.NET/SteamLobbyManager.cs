using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class SteamLobbyManager : MonoBehaviour
{
    public CSteamID HostId { get; private set; }
    public bool isHost;
    public UnityEvent<LobbyMemberInfo> OnLobbyMemeberUpdated = new UnityEvent<LobbyMemberInfo>();
    public UnityEvent OnLobbyHostUpdated = new UnityEvent();
    [Serializable]
    public struct LobbyMemberInfo
    {
        public CSteamID id;
        public string name;
        public bool isHost;
        public Texture2D avatar;
    }
    
    public static SteamLobbyManager Instance { get; private set; }

    public CSteamID LobbyId { get; private set; }
    private string lobbyName;
    
    
    // 콜백 핸들러들
    private Callback<LobbyCreated_t> lobbyCreatedCallback;
    private Callback<GameLobbyJoinRequested_t> lobbyJoinRequestCallback;
    private Callback<LobbyEnter_t> lobbyEnterCallback;
    private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback;
    private Callback<LobbyDataUpdate_t> lobbyDataCallback;

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
        lobbyDataCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
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

    public void JoinLobby(CSteamID lobbyId)
    {
        SteamMatchmaking.JoinLobby(lobbyId);
        SceneMangaer.Instance.LoadGameScene("LobbyScene");
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
    
        if (avatarHandle == -1) // 로딩 중
        {
            Debug.Log("아바타 로딩 중...");
            return null; // 아직 로드 안됨
        }
    
        if (avatarHandle == 0) // 아바타 없음
        {
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
        }
        else
        {
            Debug.LogError("로비 생성 실패!");
        }
    }

    // lobby join request 외부(초대링크, 게임참가 등)에서 join할시 콜백 (!host용)
    private void OnLobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        JoinLobby(callback.m_steamIDLobby);
    }

    // lobby enter시 콜백 (client)
    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        
        if (callback.m_EChatRoomEnterResponse == 1)
        {
            HostId = SteamMatchmaking.GetLobbyOwner(LobbyId);

            Debug.Log("로비 입장 성공!");
            // SteamNetworking.CloseP2PSessionWithUser(HostId);
            // Debug.Log("P2P 세션 강제 종료");
        }
        else
        {
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
        
        LobbyMemberInfo memberInfo = GetLobbyMember(userId);
        
        OnLobbyMemeberUpdated?.Invoke(memberInfo);
        // 입장 체크
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered) != 0)
        {
            
        }
        else
        {
            CheckHostStatus();
        }
    
        // 퇴장 체크  
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft) != 0)
        {
            
        }
    
        // 연결 끊김
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) != 0)
        {
            
        }
    
        // 강제 퇴장
        if ((stateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeKicked) != 0)
        {
            
        }
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
    {
        CheckHostStatus();
    }

    private void CheckHostStatus()
    {
        if (HostId != SteamMatchmaking.GetLobbyOwner(LobbyId))
        {
            HostId = SteamMatchmaking.GetLobbyOwner(LobbyId);
            OnLobbyHostUpdated?.Invoke();
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