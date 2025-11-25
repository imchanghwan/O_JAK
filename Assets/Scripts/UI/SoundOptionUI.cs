using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SoundOptionUI : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [SerializeField] private Text masterVolumeText;
    [SerializeField] private Text bgmVolumeText;
    [SerializeField] private Text sfxVolumeText;

    private void Start()
    {
        LoadVolumeSettings();
        
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    private void LoadVolumeSettings()
    {
        // 저장된 볼륨 불러오기 (기본값 0.75)
        float masterVolume = PlayerPrefs.GetFloat("Master", 0.75f);
        float bgmVolume = PlayerPrefs.GetFloat("BGM", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFX", 0.75f);
        
        // 슬라이더 설정
        masterVolumeSlider.value = masterVolume;
        bgmVolumeSlider.value = bgmVolume;
        sfxVolumeSlider.value = sfxVolume;
        
        // 오디오 믹서 적용
        SetMasterVolume(masterVolume);
        SetBgmVolume(bgmVolume);
        SetSfxVolume(sfxVolume);
    }

    private void SetMasterVolume(float volume)
    {
        float dB = VolumeToDecibel(volume);
        audioMixer.SetFloat("Master", dB);
        
        // 텍스트 업데이트
        if (masterVolumeText != null)
        {
            masterVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
        
        // 저장
        PlayerPrefs.SetFloat("Master", volume);
    }

    private void SetBgmVolume(float volume)
    {
        float dB = VolumeToDecibel(volume);
        audioMixer.SetFloat("BGM", dB);
        
        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
        
        PlayerPrefs.SetFloat("BGM", volume);
    }
    
    private void SetSfxVolume(float volume)
    {
        float dB = VolumeToDecibel(volume);
        audioMixer.SetFloat("SFX", dB);
        
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
        
        PlayerPrefs.SetFloat("SFX", volume);
    }
    
    // 0~1 범위 dB 단위 변환
    private float VolumeToDecibel(float volume)
    {
        if (volume <= 0f)
            return -80f; // 최소값 (음소거)
        
        return Mathf.Log10(volume) * 20f;
    }
}
