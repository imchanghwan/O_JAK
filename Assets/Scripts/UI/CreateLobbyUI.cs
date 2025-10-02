using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private InputField lobbyNameInputField;
    [SerializeField] private Selector selector;
    [SerializeField] private Button createButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        lobbyNameInputField.text = $"{SteamFriends.GetPersonaName()}'s Lobby";
        
        createButton.onClick.AddListener(CreateLobby);
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void CreateLobby()
    {
        ELobbyType lobbyType = GetLobbyType();
        string lobbyName = lobbyNameInputField.text;
        
        SteamLobbyManager.Instance.CreateLobby(lobbyType, 2, lobbyName);
        SceneMangaer.Instance.LoadGameScene("LobbyScene");
    }

    private ELobbyType GetLobbyType()
    {
        string lobbyType = selector.GetCurrentOption();
        
        switch (lobbyType)
        {
            case "Public":
                return ELobbyType.k_ELobbyTypePublic;
            case "Private":
                return ELobbyType.k_ELobbyTypePrivate;
            case "Friends Only":
                return ELobbyType.k_ELobbyTypeFriendsOnly;
            default:
                return ELobbyType.k_ELobbyTypePublic;
        }
    }
}
