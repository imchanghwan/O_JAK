using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] private Button backButton;
    
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private Toggle displayToggle;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle languageToggle;
    
    [SerializeField] private GameObject displayOptionPanel;
    [SerializeField] private GameObject soundOptionPanel;
    [SerializeField] private GameObject languageOptionPanel;
    
    private Dictionary<Toggle, GameObject> togglePanelMap;

    private void Awake()
    {
        togglePanelMap = new Dictionary<Toggle, GameObject>
        {
            { displayToggle, displayOptionPanel },
            { soundToggle, soundOptionPanel },
            { languageToggle, languageOptionPanel }
        };
    }

    private void Start()
    {
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
        
        displayToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(displayToggle, isOn));
        soundToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(soundToggle, isOn));
        languageToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(languageToggle, isOn));
    }

    private void OnToggleChanged(Toggle selectedToggle, bool isOn)
    {
        if (!isOn) return;
        foreach (var panel in togglePanelMap.Values)
        {
            panel.SetActive(false);
        }
        togglePanelMap[selectedToggle].SetActive(true);
    }
}
