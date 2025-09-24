using Steamworks;
using UnityEngine;
using System.Collections.Generic;

public class SteamP2PManager : MonoBehaviour
{
    private CSteamID remoteSteamId;
    private bool isConnected = false;
    
    // 콜백
    private Callback<P2PSessionRequest_t> p2pSessionRequestCallback;
    
    // 메시지 큐
    private Queue<NetworkMessage> messageQueue = new Queue<NetworkMessage>();
    
    [System.Serializable]
    public class NetworkMessage
    {
        public string messageType;
        public Vector3 position;
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
        p2pSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    }
    
    public void StartP2PConnection()
    {
        // 로비에서 상대방 Steam ID 가져오기
        CSteamID lobbyId = GetComponent<SteamLobbyManager>().LobbyId;
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        
        for (int i = 0; i < numMembers; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
            if (memberId != SteamUser.GetSteamID())
            {
                remoteSteamId = memberId;
                Debug.Log($"P2P 상대방 찾음: {remoteSteamId}");
                
                // 연결 테스트 메시지 전송
                SendTestMessage();
                break;
            }
        }
    }
    
    void OnP2PSessionRequest(P2PSessionRequest_t callback)
    {
        Debug.Log($"P2P 세션 요청 받음: {callback.m_steamIDRemote}");
        
        // 세션 수락
        SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
        remoteSteamId = callback.m_steamIDRemote;
        isConnected = true;
    }
    
    void Update()
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
    
    void ProcessMessages()
    {
        while (messageQueue.Count > 0)
        {
            NetworkMessage message = messageQueue.Dequeue();
            
            switch (message.messageType)
            {
                case "PlayerMove":
                    // 상대방 플레이어 위치 업데이트
                    UpdateRemotePlayerPosition(message.position);
                    break;
                    
                case "GameAction":
                    // 게임 액션 처리
                    Debug.Log($"게임 액션 받음: {message.data}");
                    break;
                    
                case "TestMessage":
                    Debug.Log("P2P 연결 테스트 성공!");
                    isConnected = true;
                    break;
            }
        }
    }
    
    public void SendPlayerPosition(Vector3 position)
    {
        if (!isConnected) return;
        
        NetworkMessage message = new NetworkMessage
        {
            messageType = "PlayerMove",
            position = position
        };
        
        SendMessage(message);
    }
    
    public void SendGameAction(string actionData)
    {
        if (!isConnected) return;
        
        NetworkMessage message = new NetworkMessage
        {
            messageType = "GameAction",
            data = actionData
        };
        
        SendMessage(message);
    }
    
    void SendTestMessage()
    {
        NetworkMessage message = new NetworkMessage
        {
            messageType = "TestMessage",
            data = "Hello P2P!"
        };
        
        SendMessage(message);
    }
    
    void SendMessage(NetworkMessage message)
    {
        if (!remoteSteamId.IsValid()) return;
        
        byte[] data = message.ToBytes();
        
        bool success = SteamNetworking.SendP2PPacket(
            remoteSteamId,
            data,
            (uint)data.Length,
            EP2PSend.k_EP2PSendReliable
        );
        
        if (!success)
        {
            Debug.LogError("P2P 메시지 전송 실패!");
        }
    }
    
    void UpdateRemotePlayerPosition(Vector3 position)
    {
        // 상대방 플레이어 오브젝트 위치 업데이트
        GameObject remotePlayer = GameObject.FindGameObjectWithTag("RemotePlayer");
        if (remotePlayer != null)
        {
            remotePlayer.transform.position = position;
        }
    }
}