using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class DisplayOptionUI : MonoBehaviour
{

    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Dropdown screenModeDropdown;
    [SerializeField] private Dropdown frameRateDropdown;
    
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private List<int> frameRateOptions = new List<int>();

    private void Start()
    {
        SetupResolutionDropdown();
        SetupScreenModeDropdown();
        SetupFrameRateDropdown();
    }

    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        

        for (int i = 0; i < resolutions.Length; i++)
        {
            float aspectRatio = (float)resolutions[i].width / resolutions[i].height;
            bool is16by9 = Mathf.Approximately(aspectRatio, 16f / 9f);
            
            if (!is16by9) continue;
            
            bool isDuplicate = false;
            foreach (var res in filteredResolutions)
            {
                if (res.width == resolutions[i].width && res.height == resolutions[i].height)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                filteredResolutions.Add(resolutions[i]);
            }
        }
        
        filteredResolutions.Sort((a, b) => {
            if (a.width != b.width)
                return b.width.CompareTo(a.width);
            else
                return b.height.CompareTo(a.height);
        });
        
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + " x " + filteredResolutions[i].height;
            options.Add(option);
        
            if (filteredResolutions[i].width == Screen.width && 
                filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        RectTransform template = resolutionDropdown.template;
        SetSize(template, filteredResolutions.Count, 40f);

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void SetupScreenModeDropdown()
    {
        screenModeDropdown.ClearOptions();

        List<string> options = new List<string>
        {
            "Fullscreen",
            "Borderless",
            "windowed"
        };
        
        screenModeDropdown.AddOptions(options);

        int currentIndex = Screen.fullScreenMode switch
        {
            FullScreenMode.ExclusiveFullScreen => 0,
            FullScreenMode.Windowed => 1,
            FullScreenMode.FullScreenWindow => 2,
            _ => 0
        };
        
        RectTransform template = screenModeDropdown.template;
        SetSize(template, options.Count, 40f);
        
        
        screenModeDropdown.value = currentIndex;
        screenModeDropdown.RefreshShownValue();
        
        screenModeDropdown.onValueChanged.AddListener(OnScreenChanged);
    }
    
    private void SetupFrameRateDropdown()
    {
        frameRateDropdown.ClearOptions();
        frameRateOptions.Clear();
        
        HashSet<int> frameRates = new HashSet<int>();
        
        int maxSupportedFrameRate = 0;
        Resolution[] resolutions = Screen.resolutions;
        foreach (var res in resolutions)
        {
            if (res.width == Screen.width && res.height == Screen.height)
            {
                int refreshRate = Mathf.RoundToInt((float)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator);
                frameRates.Add(refreshRate);

                if (refreshRate > maxSupportedFrameRate)
                {
                    maxSupportedFrameRate = refreshRate;
                }
            }
        }
        
        int[] defaultOptions = { 144, 60, 30 };
        foreach (int fps in defaultOptions)
        {
            if (fps <= maxSupportedFrameRate)
            {
                frameRates.Add(fps);
            }
        }

        frameRates.Add(-1);
        frameRateOptions = frameRates.OrderByDescending(x => x).ToList();
        
        List<string> options = new List<string>();
        int currentIndex = 0;
        
        for (int i = 0; i < frameRateOptions.Count; i++)
        {
            string option;
            if (frameRateOptions[i] == -1)
            {
                option = "제한 없음";
            }
            else
            {
                option = frameRateOptions[i] + " FPS";
            }
            options.Add(option);
            
            if (Application.targetFrameRate == frameRateOptions[i] ||
                (Application.targetFrameRate <= 0 && frameRateOptions[i] == -1))
            {
                currentIndex = i;
            }
        }

        RectTransform template = frameRateDropdown.template;
        SetSize(template, frameRateOptions.Count, 40f);
        
        frameRateDropdown.AddOptions(options);
        frameRateDropdown.value = currentIndex;
        frameRateDropdown.RefreshShownValue();
        
        frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);

    }
    
    private void OnResolutionChanged(int index)
    {
        Resolution resolution = filteredResolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    private void OnScreenChanged(int index)
    {
        Screen.fullScreenMode = index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.FullScreenWindow,
            _ => Screen.fullScreenMode
        };
    }
    private void OnFrameRateChanged(int index)
    {
        int targetFrameRate = frameRateOptions[index];
        
        if (targetFrameRate == -1)
        {
            Application.targetFrameRate = -1;
        }
        else
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
    
    private void SetSize(RectTransform template, int itemCount, float itemHeight)
    {
        float maxHeight = itemHeight * 10;
        float neededHeight = itemCount * itemHeight;
        template.sizeDelta = new Vector2(template.sizeDelta.x, Mathf.Min(neededHeight, maxHeight));
    }
}
