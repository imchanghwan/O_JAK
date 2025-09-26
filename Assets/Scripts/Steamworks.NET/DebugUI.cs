using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private Text debugText;
    
    void Start()
    {
        Application.logMessageReceived += ShowLogOnUI;
    }
    
    void ShowLogOnUI(string logString, string stackTrace, LogType type)
    {
        debugText.text += $"{logString}\n";
    }
}