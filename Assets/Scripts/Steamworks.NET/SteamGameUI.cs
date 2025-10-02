using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamGameUI : MonoBehaviour
{
    [System.Serializable]
    public struct LobbyButtonData
    {
        public Button button;
        public ELobbyType lobbyType;
        public int maxMembers;
    }
    [SerializeField] private List<LobbyButtonData> createLobbyButtons;
    
    [SerializeField] private Button joinLobbyByIdButton;
    [SerializeField] private Button inviteFriendButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button startGameButton;
    
    [SerializeField] private Text lobbyStatusText;
    [SerializeField] private Text joinStatusText;
    [SerializeField] private Text lobbyIdText;
    
    [SerializeField] private InputField lobbyIdInput;
    
    
    private SteamLobbyManager lobbyManager;
    private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;

    private void Start()
    {
        lobbyManager = FindObjectOfType<SteamLobbyManager>();
        lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        
        // 로비 status event
        lobbyManager.lobbyStatusEvent.AddListener((status) => {
            lobbyStatusText.text = status.ToString();
        });

        
        // lobby join status event
        lobbyManager.joinStatusEvent.AddListener((status) => {
            joinStatusText.text = status.ToString();
        });
        
        startGameButton.onClick.AddListener(() =>
        {
            lobbyManager.StartGame();
        });
        
        // invite friend
        inviteFriendButton.onClick.AddListener(() => { lobbyManager.InviteFriend(); });
        
        // join lobby
        joinLobbyByIdButton.onClick.AddListener(JoinLobbyById);

        leaveLobbyButton.onClick.AddListener(() => { lobbyManager.LeaveLobby(); });
    }
    
    private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
    {
        lobbyIdText.text = $"LOBBY ID : {callback.m_ulSteamIDLobby}";
    }

    private void JoinLobbyById()
    {
        string inputText = lobbyIdInput.text;
        Debug.Log($"입력된 텍스트: '{inputText}'");
        Debug.Log($"텍스트 길이: {inputText.Length}");

        if (ulong.TryParse(lobbyIdInput.text, out ulong lobbyId))
        {
            CSteamID steamId = new CSteamID(lobbyId);
            Debug.Log($"SteamID 유효성: {steamId.IsValid()}");
            lobbyManager.JoinLobbyById(steamId);
        }
        else
        {
            Debug.LogError("잘못된 로비 ID");
        }
    }
}