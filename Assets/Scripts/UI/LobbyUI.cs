using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button inviteFriendButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button leaveLobbyButton;
    
    [Header("Lobby Chat UI")]
    [SerializeField] private Transform chatContent;
    [SerializeField] private GameObject chatPrefab;
    [SerializeField] private InputField lobbyChatInputField;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button enterChatButton;
    [SerializeField] private int maxChatMessages = 100;
    
    [Header("Player List UI")]
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerListPrefab;
    
    
    private LobbyChatManager lobbyChatManager;
    private Queue<GameObject> chatMessages = new Queue<GameObject>();
    private Dictionary<CSteamID, GameObject> playerList = new Dictionary<CSteamID, GameObject>();

    private void Awake()
    {
        lobbyChatManager = GetComponent<LobbyChatManager>();
    }

    private void Start()
    {
        // main ui
        inviteFriendButton.onClick.AddListener(InviteFriend);
        leaveLobbyButton.onClick.AddListener(LeaveLobby);
        
        lobbyChatInputField.onEndEdit.AddListener(OnChatInputEnd);
        lobbyChatManager.OnLobbyChatUpdated.AddListener(UpdateLobbyChat);
        
        SteamLobbyManager.Instance.OnJoinLobby.AddListener(AddPlayerList);
        SteamLobbyManager.Instance.OnLeaveLobby.AddListener(RemovePlayerList);
        SteamLobbyManager.Instance.OnClientJoinLobby.AddListener(UpdateAllPlayersList);
    }

    // chat old input
    private void Update()
    {
        // 포커스되지 않은 상태에서만 엔터키로 활성화
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !lobbyChatInputField.isFocused && lobbyChatInputField.text.Length > 1)
        {
            lobbyChatInputField.Select();
            lobbyChatInputField.ActivateInputField();
            Debug.Log(true);
        }
        else if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && lobbyChatInputField.isFocused && lobbyChatInputField.text.Length < 1)
        {
            lobbyChatInputField.DeactivateInputField();
            Debug.Log(false);
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && lobbyChatInputField.isFocused)
        {
            lobbyChatInputField.DeactivateInputField();
            Debug.Log(false);
        }
    }

    // client 최초 입장시
    private void UpdateAllPlayersList(List<SteamLobbyManager.LobbyMemberInfo> lobbyMembers)
    {
        foreach (SteamLobbyManager.LobbyMemberInfo member in lobbyMembers)
        {
            AddPlayerList(member);
        }
    }

    private void UpdatePlayerList(SteamLobbyManager.LobbyMemberInfo lobbyMember)
    {
        
    }

    private void AddPlayerList(SteamLobbyManager.LobbyMemberInfo lobbyMember)
    {
        GameObject newPlayer = Instantiate(playerListPrefab, playerListContent);
        
        Text playerNameText = newPlayer.transform.Find("PlayerNameText").GetComponent<Text>();
        Text hostText = newPlayer.transform.Find("HostText").GetComponent<Text>();
        Transform hostButtons = newPlayer.transform.Find("HostButtons");
        RawImage playerImage = newPlayer.transform.Find("PlayerImage").GetComponent<RawImage>();


        playerList.Add(lobbyMember.id, newPlayer);
        if (lobbyMember.isHost)
        {
            hostText.gameObject.SetActive(true);
        }
        
        // client 본인이 host이면서 플레이어패널 id가 본인이 아닌 패널에 호스트 버튼 활성화
        if (SteamLobbyManager.Instance.IsHost(SteamUser.GetSteamID()) && lobbyMember.id != SteamUser.GetSteamID())
        {
            hostButtons.gameObject.SetActive(true);

            Button[] buttons = hostButtons.gameObject.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.gameObject.name == "SetOwnerButton")
                {
                    button.onClick.AddListener((() => SetOwner(lobbyMember.id)));
                }
                else if (button.gameObject.name == "KickButton")
                {
                    button.onClick.AddListener(() => KickUser(lobbyMember.id));
                }
                else if (button.gameObject.name == "BanButton")
                {
                    button.onClick.AddListener(() => BanUser(lobbyMember.id));
                }
            }
        }
        
        playerNameText.text = lobbyMember.name;
        playerImage.texture = lobbyMember.avatar;
        
    }

    private void SetOwner(CSteamID steamID)
    {
        if (steamID == CSteamID.Nil) return;
        SteamMatchmaking.SetLobbyOwner(SteamLobbyManager.Instance.LobbyId, steamID);
    }

    private void KickUser(CSteamID steamID)
    {
        
    }

    private void BanUser(CSteamID steamID)
    {
        
    }

    private void RemovePlayerList(SteamLobbyManager.LobbyMemberInfo lobbyMember)
    {
        Destroy(playerList[lobbyMember.id]);
        playerList.Remove(lobbyMember.id);
    }
    
    private void InviteFriend()
    {
        SteamLobbyManager.Instance.InviteFriend();
        // TODO : 함수 bool타입이라 실패시 피드백 메시지 띄워야됨
    }
    
    private void LeaveLobby()
    {
        // 모든 데이터 초기화
        ResetAllData();
        SteamMatchmaking.LeaveLobby(SteamLobbyManager.Instance.LobbyId);
        SceneMangaer.Instance.LoadGameScene("MainScene");
    }

    private void ResetAllData()
    {
        foreach (Transform child in chatContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in playerListContent.transform)
        {
            Destroy(child.gameObject);
        }
        
        chatMessages.Clear();
        playerList.Clear();
    }

    private void OnChatInputEnd(string input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            string message = input.Trim();
        
            if (message.Length > 0)
            {
                lobbyChatManager.SendLobbyChatMessage(message);
                lobbyChatInputField.text = "";
                lobbyChatInputField.ActivateInputField();
                Debug.Log(true);
            }
            else
            {
                lobbyChatInputField.DeactivateInputField();
                Debug.Log(false);
            }
        }
    }

    private void UpdateLobbyChat(LobbyChatManager.ChatInfo chatInfo)
    {
        GameObject newChat = Instantiate(chatPrefab, chatContent);
        
        Text nameText = newChat.transform.Find("PlayerName Text").GetComponent<Text>();
        Text chatText = newChat.transform.Find("Chat Text").GetComponent<Text>();

        nameText.text = chatInfo.playerName;
        chatText.text = chatInfo.message;
        
        chatMessages.Enqueue(newChat);

        if (chatMessages.Count > maxChatMessages)
        {
            Destroy(chatMessages.Dequeue());
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent.GetComponent<RectTransform>());
        
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

}
