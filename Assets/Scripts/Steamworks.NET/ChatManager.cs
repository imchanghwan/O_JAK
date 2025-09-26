using System;
using System.Text;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    private StringBuilder chatHistory = new StringBuilder();
    

    public void AddMessage(string playerName, string message)
    {
        chatHistory.AppendLine($"{playerName}: {message}");
    }
}