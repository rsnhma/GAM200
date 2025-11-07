using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource ambienceSource;
    public AudioSource sfxSource;

    [Header("Ambience")]
    public AudioClip ambienceClip;

    [Header("UI SFX")]
    public AudioClip SFX_Click;       // For clicking journal items, menu items, etc.
    public AudioClip SFX_Interact;    // For items in game sce aka Bulletin, Table Scribble

    [Header("Journal SFX")]
    public AudioClip SFX_Equip;
    public AudioClip SFX_Journal;     // Tab switching

    [Header("Interaction SFX")]
    public AudioClip SFX_PickUp;      // For picking up items, puzzle pieces, interactables
    public AudioClip SFX_Door;


    [Header("Puzzle SFX")]
    public AudioClip SFX_WellFail;
    public AudioClip SFX_PuzzleFail;
    public AudioClip SFX_PuzzleSuccess;

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

        // Get or create SFX source
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.7f;
        }
    }

    // ========== AMBIENCE METHODS ==========
    public void PlayAmbience(AudioClip clip)
    {
        if (ambienceSource != null && clip != null)
        {
            ambienceSource.clip = clip;
            ambienceSource.Play();
        }
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

    // ========== SFX METHODS ==========
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

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    // ========== SPECIFIC SFX METHODS ==========

    // UI sounds
    public void PlayClickSound()
    {
        PlaySFX(SFX_Click);
    }

    public void PlayInteractSound()
    {
        PlaySFX(SFX_Interact);
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
}