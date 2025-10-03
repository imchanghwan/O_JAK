using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class OnlineGameUI : MonoBehaviour
{
    [Header("━━━━━ Online Game Menu ━━━━━")]
    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private InputField lobbyIdInputField;
    [SerializeField] private Button joinLobbyByIdButton;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Button reloadButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button createLobbyButton;
    [Header("Panel GameObject")]
    [SerializeField] private GameObject createLobbyPanel;
    [Header("Lobby List")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject lobbyPrefab;
    
    private LobbyListManager lobbyListManager;
    private ToggleGroup toggleGroup;
    private CSteamID selectedLobbyId;
    
    // Lobby List Manager의 lobby list 요청이 OnEnable 함수에서 실행돼서 전에
    // Component를 Awake 에서 초기화
    // toggleGroup은 안전하게 Awake에서 초기화
    private void Awake()
    {
        lobbyListManager = GetComponent<LobbyListManager>();
        toggleGroup = content.GetComponent<ToggleGroup>();
    }

    // Lobby List Manager는 Online Panel이 활성화 됐을 때 lobby list 요청
    // 요청 이후 lobby list 수신 이벤트 함수 실행
    private void OnEnable()
    {
        lobbyIdInputField.text = "";
        feedbackText.text = "";
        
        lobbyListManager.RequestLobbyList();
        lobbyListManager.OnLobbyListUpdated.AddListener(UpdateLobbyList);
    }
    
    // Buttons OnClick Listener
    private void Start()
    {
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
        reloadButton.onClick.AddListener(lobbyListManager.RequestLobbyList);
        joinLobbyButton.onClick.AddListener(JoinSelectedLobby);
        joinLobbyByIdButton.onClick.AddListener(JoinLobbyById);
        createLobbyButton.onClick.AddListener(() => createLobbyPanel.SetActive(true));
    }

    private void OnDisable()
    {
        lobbyListManager.OnLobbyListUpdated.RemoveListener(UpdateLobbyList);
    }

    private void JoinLobbyById()
    {
        if (ulong.TryParse(lobbyIdInputField.text, out ulong lobbyId))
        {
            CSteamID steamId = new CSteamID(lobbyId);
            if (!steamId.IsValid())
            {
                feedbackText.text = "Invalid lobby Id";
                return;
            }

            SteamLobbyManager.Instance.JoinLobby(steamId);
        }
        else
        {
            feedbackText.text = "잘못된 로비 ID";
        }
    }
    
    private void UpdateLobbyList(List<LobbyListManager.LobbyInfo> lobbyList)
    {
        // reset
        selectedLobbyId = new CSteamID();
        joinLobbyButton.interactable = false;
        
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var lobbyInfo in lobbyList)
        {
            CreateLobbyItem(lobbyInfo);
        }
    }

    private void CreateLobbyItem(LobbyListManager.LobbyInfo lobbyInfo)
    {
        GameObject lobbyItem = Instantiate(lobbyPrefab, content);
        
        Toggle toggle = lobbyItem.GetComponent<Toggle>();
        Text lobbyNameText = lobbyItem.transform.Find("Lobby Name Text").GetComponent<Text>();
        Text membersText = lobbyItem.transform.Find("Members Text").GetComponent<Text>();
        
        toggle.group = toggleGroup;
        
        lobbyNameText.text = lobbyInfo.lobbyName;
        membersText.text = $"{lobbyInfo.currentMembers} / {lobbyInfo.maxMembers}";
        
        toggle.onValueChanged.AddListener((isOn) => OnToggleChanged(isOn, lobbyInfo));
    }

    private void OnToggleChanged(bool isOn, LobbyListManager.LobbyInfo lobbyInfo)
    {
        if (isOn)
        {
            selectedLobbyId = lobbyInfo.lobbyID;
            Debug.Log($"선택된 로비: {lobbyInfo.lobbyID}");
        }
        
        UpdateJoinButtonState();
    }

    private void UpdateJoinButtonState()
    {
        joinLobbyButton.interactable = toggleGroup.AnyTogglesOn();
    }

    private void JoinSelectedLobby()
    {
        if (toggleGroup.AnyTogglesOn())
        {
            SteamLobbyManager.Instance.JoinLobby(selectedLobbyId);
        }
    }
}
