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
    [SerializeField] private Button clipboardButton;
    
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
    private CSteamID lobbyID;
    private Dictionary<CSteamID, GameObject> playerList = new Dictionary<CSteamID, GameObject>();
    
    private void Awake()
    {
        lobbyChatManager = GetComponent<LobbyChatManager>();
        lobbyID = SteamLobbyManager.Instance.LobbyId;
    }

    // client 로비 들어왔을때 처리
    private void OnEnable()
    {
        List<SteamLobbyManager.LobbyMemberInfo> lobbyMembers = SteamLobbyManager.Instance.GetLobbyMembers(lobbyID);
        UpdateAllPlayerList(lobbyMembers);
        
        SteamLobbyManager.Instance.OnLobbyMemeberUpdated.AddListener(UpdateLobbyPlayerList);
        SteamLobbyManager.Instance.OnLobbyHostUpdated.AddListener(UpdateLobbyHost);
    }

    private void Start()
    {
        // main ui
        inviteFriendButton.onClick.AddListener(InviteFriend);
        leaveLobbyButton.onClick.AddListener(LeaveLobby);
        
        lobbyChatInputField.onEndEdit.AddListener(OnChatInputEnd);
        lobbyChatManager.OnLobbyChatUpdated.AddListener(UpdateLobbyChat);
        
        clipboardButton.onClick.AddListener(Clipboard);
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
    
    private void UpdateLobbyHost()
    {
        RemoveAllPlayerList();
        List<SteamLobbyManager.LobbyMemberInfo> lobbyMembers = SteamLobbyManager.Instance.GetLobbyMembers(lobbyID);
        UpdateAllPlayerList(lobbyMembers);
    }
    
    private void RemoveAllPlayerList()
    {
        playerList.Clear();
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void UpdateAllPlayerList(List<SteamLobbyManager.LobbyMemberInfo> lobbyMembers)
    {
        Debug.Log("UpdateAllPlayersList");
        foreach (SteamLobbyManager.LobbyMemberInfo member in lobbyMembers)
        {
            AddPlayerList(member);
        }
    }

    private void UpdateLobbyPlayerList(SteamLobbyManager.LobbyMemberInfo member)
    {
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
        if (playerList.Count < memberCount) // 로비 멤버 감소
        {
            AddPlayerList(member);
        }
        else if (playerList.Count > memberCount) // 로비 멤버 증가
        {
            RemovePlayerList(member.id);
        }
    }
    
    private void AddPlayerList(SteamLobbyManager.LobbyMemberInfo lobbyMember)
    {
        GameObject newPlayer = Instantiate(playerListPrefab, playerListContent);
        
        Text playerNameText = newPlayer.transform.Find("PlayerNameText").GetComponent<Text>();
        Text hostText = newPlayer.transform.Find("HostText").GetComponent<Text>();
        Transform hostButtons = newPlayer.transform.Find("HostButtons");
        RawImage playerImage = newPlayer.transform.Find("PlayerImage").GetComponent<RawImage>();
        playerImage.rectTransform.localScale = new Vector3(1, -1, 1);

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

    private void RemovePlayerList(CSteamID steamID)
    {
        Destroy(playerList[steamID]);
        playerList.Remove(steamID);
    }

    private void SetOwner(CSteamID steamID)
    {
        if (!SteamLobbyManager.Instance.IsHost(SteamUser.GetSteamID())) return;
        if (steamID == CSteamID.Nil) return;
        SteamMatchmaking.SetLobbyOwner(SteamLobbyManager.Instance.LobbyId, steamID);
    }

    private void KickUser(CSteamID steamID)
    {
        if (!SteamLobbyManager.Instance.IsHost(SteamUser.GetSteamID())) return;
        if (steamID == CSteamID.Nil) return;
    }

    private void BanUser(CSteamID steamID)
    {
        if (!SteamLobbyManager.Instance.IsHost(SteamUser.GetSteamID())) return;
        if (steamID == CSteamID.Nil) return;
        
    }

    private void InviteFriend()
    {
        SteamLobbyManager.Instance.InviteFriend();
        // TODO : 함수 bool타입이라 실패시 피드백 메시지 띄워야됨
    }
    
    private void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(SteamLobbyManager.Instance.LobbyId);
        SceneMangaer.Instance.LoadGameScene("MainScene");
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

    private void Clipboard()
    {
        GUIUtility.systemCopyBuffer = lobbyID.ToString();
        Debug.Log($"Clipboard {lobbyID}");
    }
}
