using System;
using Steamworks;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LobbyChatManager : MonoBehaviour
{
    [Serializable]
    public struct ChatInfo
    {
        public string playerName;
        public string message;
    }
    
    private StringBuilder chatHistory = new StringBuilder();
    private Callback<LobbyChatMsg_t> lobbyChatMsgCallback;
    
    public UnityEvent<ChatInfo> OnLobbyChatUpdated = new UnityEvent<ChatInfo>();

    private void Start()
    {
        lobbyChatMsgCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
    }
    
    public void SendLobbyChatMessage(string message)
    {
        CSteamID lobbyId = SteamLobbyManager.Instance.LobbyId;
        
        byte[] data = Encoding.UTF8.GetBytes(message);
        bool success = SteamMatchmaking.SendLobbyChatMsg(lobbyId, data, data.Length);
        
        if (!success)
        {
            Debug.LogError("로비 채팅 전송 실패");
        }
    }
    private void OnLobbyChatMessage(LobbyChatMsg_t callback)
    {
        CSteamID senderId = new CSteamID(callback.m_ulSteamIDUser);
    
        // 메시지 데이터 읽기
        byte[] data = new byte[1024];
        EChatEntryType chatType;
        int messageLength = SteamMatchmaking.GetLobbyChatEntry(
            new CSteamID(callback.m_ulSteamIDLobby),
            (int)callback.m_iChatID,
            out senderId,
            data,
            data.Length,
            out chatType
        );
    
        if (messageLength > 0)
        {
            string message = Encoding.UTF8.GetString(data, 0, messageLength);
            string senderName = SteamFriends.GetFriendPersonaName(senderId);


            ChatInfo chatInfo = new ChatInfo()
            {
                playerName = senderName,
                message = message
            };
            
            OnLobbyChatUpdated?.Invoke(chatInfo);
        }
    }
}