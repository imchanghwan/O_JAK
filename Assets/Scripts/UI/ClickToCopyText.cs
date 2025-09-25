using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class ClickToCopyText : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Text textComponent;
    [SerializeField] private Color feedbackColor = Color.green;
    
    private Color originalColor;
    private SteamLobbyManager lobbyManager;
    

    private void Start()
    {
        lobbyManager = FindObjectOfType<SteamLobbyManager>();
        originalColor = textComponent.color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (textComponent == null) return;
        if (lobbyManager == null) return;

        if (lobbyManager.lobbyStatus != LobbyStatus.Created) return;
        String copyText = lobbyManager.lobbyId.ToString();
        
        GUIUtility.systemCopyBuffer = copyText;
        Debug.Log($"클립보드에 복사됨: {copyText}");
        ShowCopyFeedback();
    }

    private void ShowCopyFeedback()
    {
        StartCoroutine(CopyFeedbackCoroutine());
    }

    private IEnumerator CopyFeedbackCoroutine()
    {
        textComponent.color = feedbackColor;
        
        yield return new WaitForSeconds(0.3f);
        
        textComponent.color = originalColor;
    }
}