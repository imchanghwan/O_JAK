using System.Collections;
using Steamworks;
using UnityEngine;
using System.Collections.Generic;

public class SteamP2PManager : MonoBehaviour
{
    private CSteamID lobbyId;
    
    // 콜백
    private Callback<P2PSessionRequest_t> p2pSessionRequestCallback;
    private Callback<P2PSessionConnectFail_t> p2pSessionConnectFail;
    
    // 메시지 큐
    private Queue<NetworkMessage> messageQueue = new Queue<NetworkMessage>();

    public enum MessageType
    {
        Message,
    }
    
    [System.Serializable]
    public class NetworkMessage
    {
        public MessageType messageType;
        public Vector3 position;
        public CSteamID steamId;
        public string data;
        
        public byte[] ToBytes()
        {
            string json = JsonUtility.ToJson(this);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
        
        public static NetworkMessage FromBytes(byte[] bytes)
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<NetworkMessage>(json);
        }
    }
    void Start()
    {
        if (!SteamManager.Instance.Initialized) return;
    
        p2pSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
        p2pSessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
    }
    
    // host send message (when players send messages to host)
    public void HostSendMessage(NetworkMessage message)
    {
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(SteamLobbyManager.lobbyId);
        Debug.Log($"lobby members : {numMembers}");
        
        for (int i = 0; i < numMembers; i++)
        {
            Debug.Log("host send message for");
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobbyManager.lobbyId, i);
            // host 본인 제외 전송
            if (memberId != SteamUser.GetSteamID())
            {
                Debug.Log($"P2P 상대방 찾음: {memberId}");
                
                SendMessage(memberId, message);
            }
        }
    }
    
    public void SendMessage(CSteamID steamId, NetworkMessage message)
    {
        if (!steamId.IsValid()) 
        {
            Debug.LogError("상대방 Steam ID 무효!");
            return;
        }
        
        byte[] data = message.ToBytes();
        Debug.Log($"패킷 전송 시도: 크기={data.Length}, 상대방={steamId}");
        
        bool success = SteamNetworking.SendP2PPacket(
            steamId,
            data,
            (uint)data.Length,
            EP2PSend.k_EP2PSendReliable
        );
        Debug.Log("P2P 메시지 전송 성공");
        
        if (!success)
        {
            Debug.LogError("P2P 메시지 전송 실패!");
        }
    }
    
    private void OnP2PSessionRequest(P2PSessionRequest_t callback)
    {
        lobbyId = SteamLobbyManager.lobbyId;
        Debug.Log($"P2P 세션 요청 받음: {callback.m_steamIDRemote}");
        
        // 세션 수락
        SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
        //remoteSteamId = callback.m_steamIDRemote;
    }

    private void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback)
    {
        CSteamID failedSteamId = callback.m_steamIDRemote;
        string playerName = SteamFriends.GetFriendPersonaName(failedSteamId);
        EP2PSessionError error = (EP2PSessionError)callback.m_eP2PSessionError;
    
        Debug.LogError($"P2P 연결 실패: {playerName} - 오류 코드: {error}");
    
        switch (error)
        {
            case EP2PSessionError.k_EP2PSessionErrorNone:
                Debug.Log("오류 없음");
                break;
            case EP2PSessionError.k_EP2PSessionErrorNoRightsToApp:
                Debug.LogError("로컬 사용자가 앱을 소유하지 않음");
                break;
            case EP2PSessionError.k_EP2PSessionErrorTimeout:
                Debug.LogError("연결 시간 초과");
                break;
            default:
                Debug.LogError($"알 수 없는 오류: {error}");
                break;
        }

    }
    
    private void Update()
    {
        // 메시지 수신 확인
        uint msgSize;
        while (SteamNetworking.IsP2PPacketAvailable(out msgSize))
        {
            byte[] data = new byte[msgSize];
            uint bytesRead;
            CSteamID senderId;
            
            if (SteamNetworking.ReadP2PPacket(data, msgSize, out bytesRead, out senderId))
            {
                NetworkMessage message = NetworkMessage.FromBytes(data);
                messageQueue.Enqueue(message);
            }
        }
        
        // 받은 메시지 처리
        ProcessMessages();
    }
    
    private void ProcessMessages()
    {
        while (messageQueue.Count > 0)
        {
            NetworkMessage message = messageQueue.Dequeue();
            
            // if host
            if (SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(lobbyId))
            {
                HostSendMessage(message);
            }
            
            string senderName = SteamFriends.GetFriendPersonaName(message.steamId);
            
            switch (message.messageType)
            {
                case MessageType.Message:
                    Debug.Log($"{senderName} : {message.data}");
                    break;
            }
        }
    }
    
}