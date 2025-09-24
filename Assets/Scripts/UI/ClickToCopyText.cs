using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
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

        String copyText = lobbyManager.LobbyId.ToString();
        if (copyText == "0") return;
        
        GUIUtility.systemCopyBuffer = copyText;
        Debug.Log($"클립보드에 복사됨: {copyText}");
        ShowCopyFeedback();
    }
    
    void ShowCopyFeedback()
    {
        StartCoroutine(CopyFeedbackCoroutine());
    }
    
    System.Collections.IEnumerator CopyFeedbackCoroutine()
    {
        textComponent.color = feedbackColor;
        
        yield return new WaitForSeconds(0.3f);
        
        textComponent.color = originalColor;
    }
}