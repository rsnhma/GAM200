using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        InitializeSliders();
    }

    private void OnEnable()
    {
        // Refresh values when panel opens (in case changed elsewhere)
        InitializeSliders();
    }

    private void InitializeSliders()
    {
        if (SoundManager.Instance == null) return;

        // Set slider values to match saved settings
        if (masterSlider != null)
        {
            masterSlider.value = SoundManager.Instance.GetMasterVolume();
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.value = SoundManager.Instance.GetMusicVolume();
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = SoundManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        SoundManager.Instance?.SetMasterVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        SoundManager.Instance?.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance?.SetSFXVolume(value);
    }
}