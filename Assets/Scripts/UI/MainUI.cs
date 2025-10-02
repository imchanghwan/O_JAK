using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [Header("━━━━━ Title Menu ━━━━━")] 
    [Header("Button")] 
    [SerializeField] private Button offlinePlayButton;
    [SerializeField] private Button onlinePlayButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;

    [Header("Panel GameObject")] [SerializeField]
    private GameObject offlineGamePanel;

    [SerializeField] private GameObject onlineGamePanel;
    [SerializeField] private GameObject howToPlayGamePanel;
    [SerializeField] private GameObject optionsPanel;
    
    private void Start()
    {
        //offlinePlayButton.onClick.AddListener(() => SetActivePanelObject(offlineGamePanel));
        onlinePlayButton.onClick.AddListener(() => onlineGamePanel.SetActive(true));
        howToPlayButton.onClick.AddListener(() => howToPlayGamePanel.SetActive(true));
        optionsButton.onClick.AddListener(() => optionsPanel.SetActive(true));

        // 게임종료
        exitButton.onClick.AddListener(ExitGame);
    }

    private void ExitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
