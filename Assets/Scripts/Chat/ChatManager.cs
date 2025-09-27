using Steamworks;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LobbyChatManager : MonoBehaviour
{
    [SerializeField] private Text chatDisplayText;
    [SerializeField] private InputField chatInputField;
    
    private StringBuilder chatHistory = new StringBuilder();
    private bool justDeactivated = false;
    private Callback<LobbyChatMsg_t> lobbyChatMsgCallback;

    private void Start()
    {
        lobbyChatMsgCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
        
        chatInputField.onEndEdit.AddListener(OnChatInputEnd);
    }
    
    void Update()
    {
        // 포커스되지 않은 상태에서만 엔터키로 활성화
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) 
            && !chatInputField.isFocused && !justDeactivated)
        {
            chatInputField.Select();
            chatInputField.ActivateInputField();
        }
        
        if (justDeactivated && Input.GetKeyUp(KeyCode.Return))
        {
            justDeactivated = false;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && chatInputField.isFocused)
        {
            chatInputField.DeactivateInputField();
        }
    }

    void OnChatInputEnd(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            string message = input.Trim();
        
            if (message.Length > 0)
            {
                SendLobbyChatMessage(message);
                chatInputField.text = "";
                chatInputField.ActivateInputField();
            }
            else
            {
                chatInputField.DeactivateInputField();
                justDeactivated = true; // Update에서 재활성화 방지
            }
        }
    }
    public void DisplayChatMessage(string playerName, string message)
    {
        chatDisplayText.text += $"{playerName}: {message}\n";
        chatHistory.AppendLine($"{playerName}: {message}");
    }
    
    public void SendLobbyChatMessage(string message)
    {
        CSteamID lobbyId = SteamLobbyManager.lobbyId;
        
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
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
            string message = System.Text.Encoding.UTF8.GetString(data, 0, messageLength);
            string senderName = SteamFriends.GetFriendPersonaName(senderId);
        
            Debug.Log($"{senderName}: {message}");
            DisplayChatMessage(senderName, message);
        }
    }
}