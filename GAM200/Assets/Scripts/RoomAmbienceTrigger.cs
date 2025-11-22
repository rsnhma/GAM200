using UnityEngine;

public class RoomAmbienceTrigger : MonoBehaviour
{
    [Header("Room Ambience Settings")]
    [Tooltip("Primary ambience sound for this room")]
    public AudioClip roomAmbience;

    [Tooltip("Optional: Layer ambience (plays on top of primary)")]
    public AudioClip roomAmbienceLayer;

    [Header("Transition Settings")]
    [Tooltip("Use smooth crossfade instead of instant change")]
    public bool useCrossfade = true;

    [Tooltip("Duration of fade/crossfade transition")]
    public float transitionDuration = 1.5f;

    [Header("Layer Settings")]
    [Tooltip("Volume for the ambience layer (0-1)")]
    [Range(0f, 1f)]
    public float layerVolume = 0.4f;

    [Header("Trigger Settings")]
    [Tooltip("Tag required on object entering trigger (usually 'Player')")]
    public string requiredTag = "Player";

    [Header("Start Room Settings")]
    [Tooltip("Is this the starting room? (plays ambience immediately on level load)")]
    public bool isStartingRoom = false;

    private bool hasPlayedOnce = false;

    private void Start()
    {
        // If this is the starting room, play ambience immediately
        if (isStartingRoom)
        {
            Invoke(nameof(PlayStartingRoomAmbience), 0.1f);
        }
    }

    private void PlayStartingRoomAmbience()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundManager not found!");
            return;
        }

        // Play ambience immediately (no crossfade for first time)
        if (roomAmbience != null)
        {
            SoundManager.Instance.PlayAmbience(roomAmbience);
            Debug.Log($"Starting room ambience: {gameObject.name}");
        }

        // Play layer if assigned
        if (roomAmbienceLayer != null)
        {
            SoundManager.Instance.PlayAmbienceLayer(roomAmbienceLayer, layerVolume);
        }

        hasPlayedOnce = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name}: Something entered trigger - {other.gameObject.name} with tag {other.tag}");

        // Check if the correct object entered (usually the player)
        if (other.CompareTag(requiredTag))
        {
            Debug.Log($"{gameObject.name}: Player detected! Changing ambience...");
            ChangeRoomAmbience();
        }
        else
        {
            Debug.Log($"{gameObject.name}: Not the player. Required tag: {requiredTag}, Got: {other.tag}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"{gameObject.name}: Something entered 2D trigger - {other.gameObject.name} with tag {other.tag}");

        // Check if the correct object entered (usually the player)
        if (other.CompareTag(requiredTag))
        {
            Debug.Log($"{gameObject.name}: Player detected! Changing ambience...");
            ChangeRoomAmbience();
        }
        else
        {
            Debug.Log($"{gameObject.name}: Not the player. Required tag: {requiredTag}, Got: {other.tag}");
        }
    }

    private void ChangeRoomAmbience()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundManager not found!");
            return;
        }

        // Change primary ambience
        if (roomAmbience != null)
        {
            // Use crossfade only if we've already played ambience before
            if (useCrossfade && hasPlayedOnce)
            {
                SoundManager.Instance.CrossfadeAmbience(roomAmbience, transitionDuration);
            }
            else
            {
                SoundManager.Instance.PlayAmbience(roomAmbience);
                hasPlayedOnce = true;
            }
        }

        // Handle ambience layer
        if (roomAmbienceLayer != null)
        {
            SoundManager.Instance.PlayAmbienceLayer(roomAmbienceLayer, layerVolume);
        }
        else
        {
            // Stop any existing layer if this room doesn't have one
            if (hasPlayedOnce)
            {
                SoundManager.Instance.FadeOutAmbienceLayer(transitionDuration);
            }
            else
            {
                SoundManager.Instance.StopAmbienceLayer();
            }
        }

        Debug.Log($"Changed ambience for room: {gameObject.name}");
    }
}