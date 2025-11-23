using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource ambienceSource;        // Primary ambience
    public AudioSource ambienceLayerSource;   // Secondary ambience layer: For when we need to add sound layer to rooms
    public AudioSource sfxSource;
    public AudioSource menuBGMSource;

    [Header("Menu BGM")]
    public AudioClip menuBGMClip;

    [Header("Ambience")]
    public AudioClip ambienceClip;

    [Header("UI SFX")]
    public AudioClip SFX_Click;
    public AudioClip SFX_Interact;
    public AudioClip SFX_Dialogue;

    [Header("Journal SFX")]
    public AudioClip SFX_Equip;
    public AudioClip SFX_Journal;

    [Header("Interaction SFX")]
    public AudioClip SFX_PickUp;
    public AudioClip SFX_Door;

    [Header("Puzzle SFX")]
    public AudioClip SFX_WellFail;
    public AudioClip SFX_PuzzleFail;
    public AudioClip SFX_PuzzleSuccess;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;  // Controls ambience + menu BGM
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // Base volumes 
    private float baseAmbienceVolume = 0.5f;
    private float baseAmbienceLayerVolume = 0.4f;
    private float baseSFXVolume = 0.7f;
    private float baseMenuBGMVolume = 0.6f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup audio sources
        SetupAudioSources();
        LoadVolumeSettings();

        // Start playing ambience if assigned
        if (ambienceClip != null && ambienceSource != null)
        {
            PlayAmbience(ambienceClip);
        }
    }

    private void SetupAudioSources()
    {
        // Get or create ambience source
        if (ambienceSource == null)
        {
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
            ambienceSource.volume = 0.5f;
        }

        // NEW: Get or create ambience layer source
        if (ambienceLayerSource == null)
        {
            ambienceLayerSource = gameObject.AddComponent<AudioSource>();
            ambienceLayerSource.loop = true;
            ambienceLayerSource.playOnAwake = false;
            ambienceLayerSource.volume = 0.4f;
        }

        // Get or create SFX source
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.7f;
        }

        // Get or create menu BGM source
        if (menuBGMSource == null)
        {
            menuBGMSource = gameObject.AddComponent<AudioSource>();
            menuBGMSource.loop = true;
            menuBGMSource.playOnAwake = false;
            menuBGMSource.volume = 0.6f;
        }
    }

    //MENU BGM 
    public void PlayMenuBGM()
    {
        if (menuBGMSource != null && menuBGMClip != null)
        {
            menuBGMSource.clip = menuBGMClip;
            menuBGMSource.loop = true;
            menuBGMSource.Play();
            Debug.Log("Menu BGM started");
        }
    }

    public void StopMenuBGM()
    {
        if (menuBGMSource != null && menuBGMSource.isPlaying)
        {
            menuBGMSource.Stop();
            Debug.Log("Menu BGM stopped");
        }
    }

    public void FadeOutMenuBGM(float duration = 1f)
    {
        if (menuBGMSource != null)
        {
            StartCoroutine(FadeOutMenuBGMCoroutine(duration));
        }
    }

    private IEnumerator FadeOutMenuBGMCoroutine(float duration)
    {
        float startVolume = menuBGMSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            menuBGMSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        menuBGMSource.Stop();
        menuBGMSource.volume = startVolume;
        Debug.Log("Menu BGM faded out");
    }

    //AMBIENCE METHODS

    // Play single ambience (stops any existing)
    public void PlayAmbience(AudioClip clip)
    {
        if (ambienceSource != null && clip != null)
        {
            ambienceSource.clip = clip;
            ambienceSource.Play();
            Debug.Log($"Playing ambience: {clip.name}");
        }
    }

    //Play ambience with smooth crossfade
    public void CrossfadeAmbience(AudioClip newClip, float fadeDuration = 1f)
    {
        if (ambienceSource != null && newClip != null)
        {
            StartCoroutine(CrossfadeAmbienceCoroutine(newClip, fadeDuration));
        }
    }

    private IEnumerator CrossfadeAmbienceCoroutine(AudioClip newClip, float duration)
    {
        float startVolume = ambienceSource.volume;
        float elapsed = 0f;

        // Fade out current
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            ambienceSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2));
            yield return null;
        }

        // Switch clip
        ambienceSource.clip = newClip;
        ambienceSource.Play();

        // Fade in new
        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            ambienceSource.volume = Mathf.Lerp(0f, startVolume, elapsed / (duration / 2));
            yield return null;
        }

        ambienceSource.volume = startVolume;
        Debug.Log($"Crossfaded to ambience: {newClip.name}");
    }

    //Play layered ambience (plays on secondary source)
    public void PlayAmbienceLayer(AudioClip layerClip, float volume = 0.4f)
    {
        if (ambienceLayerSource != null && layerClip != null)
        {
            ambienceLayerSource.clip = layerClip;
            ambienceLayerSource.volume = volume;
            ambienceLayerSource.Play();
            Debug.Log($"Playing ambience layer: {layerClip.name}");
        }
    }

    //Stop ambience layer
    public void StopAmbienceLayer()
    {
        if (ambienceLayerSource != null && ambienceLayerSource.isPlaying)
        {
            ambienceLayerSource.Stop();
            Debug.Log("Ambience layer stopped");
        }
    }

    //Fade out ambience layer
    public void FadeOutAmbienceLayer(float duration = 1f)
    {
        if (ambienceLayerSource != null)
        {
            StartCoroutine(FadeOutAmbienceLayerCoroutine(duration));
        }
    }

    private IEnumerator FadeOutAmbienceLayerCoroutine(float duration)
    {
        float startVolume = ambienceLayerSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ambienceLayerSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        ambienceLayerSource.Stop();
        ambienceLayerSource.volume = startVolume;
        Debug.Log("Ambience layer faded out");
    }

    public void StopAmbience()
    {
        if (ambienceSource != null)
        {
            ambienceSource.Stop();
        }
    }

    public void SetAmbienceVolume(float volume)
    {
        if (ambienceSource != null)
        {
            ambienceSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetAmbienceLayerVolume(float volume)
    {
        if (ambienceLayerSource != null)
        {
            ambienceLayerSource.volume = Mathf.Clamp01(volume);
        }
    }

    //SFX METHODS
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    //SPECIFIC SFX METHODS

    // UI sounds
    public void PlayClickSound()
    {
        PlaySFX(SFX_Click);
    }

    public void PlayInteractSound()
    {
        PlaySFX(SFX_Interact);
    }

    public void PlayDialogueSound()
    {
        PlaySFX(SFX_Dialogue);
    }

    // Journal sounds
    public void PlayEquipSound()
    {
        PlaySFX(SFX_Equip);
    }

    public void PlayJournalTabSound()
    {
        PlaySFX(SFX_Journal);
    }

    // Interaction sounds
    public void PlayPickUpSound()
    {
        PlaySFX(SFX_PickUp);
    }

    // Puzzle sounds
    public void PlayWellFailSound()
    {
        PlaySFX(SFX_WellFail);
    }

    public void PlayPuzzleFailSound()
    {
        PlaySFX(SFX_PuzzleFail);
    }

    public void PlayPuzzleSuccessSound()
    {
        PlaySFX(SFX_PuzzleSuccess);
    }

    public void PlayDoorSound()
    {
        PlaySFX(SFX_Door);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyAllVolumes();
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolumes();
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplySFXVolume();
        SaveVolumeSettings();
    }

    private void ApplyAllVolumes()
    {
        ApplyMusicVolumes();
        ApplySFXVolume();
    }

    private void ApplyMusicVolumes()
    {
        if (ambienceSource != null)
            ambienceSource.volume = baseAmbienceVolume * musicVolume * masterVolume;

        if (ambienceLayerSource != null)
            ambienceLayerSource.volume = baseAmbienceLayerVolume * musicVolume * masterVolume;

        if (menuBGMSource != null)
            menuBGMSource.volume = baseMenuBGMVolume * musicVolume * masterVolume;
    }

    private void ApplySFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = baseSFXVolume * sfxVolume * masterVolume;
    }

    // Save/Load volume settings using PlayerPrefs
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ApplyAllVolumes();
    }

    // Getters for UI initialization
    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
}