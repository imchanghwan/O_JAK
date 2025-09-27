using Steamworks;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BasicChat : MonoBehaviour
{
    public Text chatDisplayText;
    public InputField chatInputField;
    
    private StringBuilder chatHistory = new StringBuilder();
    private bool chatInputActive = false;
    private void Start()
    {
        chatInputField.gameObject.SetActive(false);
    }

}
