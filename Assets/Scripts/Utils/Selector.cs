using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Selector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Text displayText;
    
    [Header("Options")]
    [SerializeField] private string[] options = { "Option1", "Option2", "Option3" };
    
    [Header("Events")]
    public UnityEvent<int> OnValueChanged; // 선택값 변경 시 호출
    public UnityEvent<string> OnOptionChanged; // 선택 옵션명 변경 시 호출
    
    private int currentIndex = 0;

    private void Start()
    {
        leftButton.onClick.AddListener(PreviousOption);
        rightButton.onClick.AddListener(NextOption);
        
        UpdateDisplay();
    }

    private void PreviousOption()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = options.Length - 1; // 처음으로 순환
        }
        
        UpdateDisplay();
        OnValueChanged?.Invoke(currentIndex);
        OnOptionChanged?.Invoke(options[currentIndex]);
    }

    private void NextOption()
    {
        currentIndex++;
        if (currentIndex >= options.Length)
        {
            currentIndex = 0; // 끝에서 처음으로 순환
        }
        
        UpdateDisplay();
        OnValueChanged?.Invoke(currentIndex);
        OnOptionChanged?.Invoke(options[currentIndex]);
    }

    private void UpdateDisplay()
    {
        displayText.text = options[currentIndex];
    }
    
    public string GetCurrentOption()
    {
        return options[currentIndex];
    }
}