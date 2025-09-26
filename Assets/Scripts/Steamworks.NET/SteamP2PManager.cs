using System.Collections;
using Steamworks;
using UnityEngine;
using System.Collections.Generic;

public class SteamP2PManager : MonoBehaviour
{
    private bool isConnected = false;
    private CSteamID lobbyId;
    
    // 콜백
    private Callback<P2PSessionRequest_t> p2pSessionRequestCallback;
    
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
    }
    
    // host send message (when players send messages to host)
    public void HostSendMessage(NetworkMessage message)
    {
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        
        for (int i = 0; i < numMembers; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
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
        lobbyId = GetComponent<SteamLobbyManager>().lobbyId;
        Debug.Log($"P2P 세션 요청 받음: {callback.m_steamIDRemote}");
        
        // 세션 수락
        SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
        //remoteSteamId = callback.m_steamIDRemote;
        isConnected = true;
    }
    
    private void Update()
    {
        if (!isConnected) return;
        
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