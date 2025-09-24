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
    [SerializeField] private Button inviteFriendButton;
    [SerializeField] private Text statusText;
    [SerializeField] private Text lobbyIdText;
    
    
    private SteamLobbyManager lobbyManager;

    private void Start()
    {
        lobbyManager = FindObjectOfType<SteamLobbyManager>();
        
        lobbyManager.lobbyEvent.AddListener((status, message) =>
        {
            statusText.text = message;
            lobbyIdText.text = $"LOBBY ID : {lobbyManager.LobbyId}";
        });
        
        foreach (LobbyButtonData lobbyButtonData in createLobbyButtons)
        {
            lobbyButtonData.button.onClick.AddListener(() =>
            {
                CreateLobby(lobbyButtonData.lobbyType, lobbyButtonData.maxMembers);
            });
        }
        
        inviteFriendButton.onClick.AddListener(() => {
            lobbyManager.InviteFriend();
            statusText.text = "친구 초대 창 열림";
        });

        
    }

    private void CreateLobby(ELobbyType lobbyType, int maxMembers)
    {
        lobbyManager.CreateLobby(lobbyType, maxMembers);
        statusText.text = $"Creating lobby {lobbyType.ToString()}";
    }
    
}