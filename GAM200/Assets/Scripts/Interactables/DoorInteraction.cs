using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip doorSound;
    [Range(0.5f, 10f)]
    public float audioPitch = 1.5f; // Adjust speed of door sound

    [Header("Animation")]
    [SerializeField] private Animator doorAnimator;

    private bool isPlaying = false;

    // Call this method from your MapTransition script
    public void PlayDoorOpenSound()
    {
        if (!isPlaying && audioSource != null && doorSound != null)
        {
            StartCoroutine(PlayDoorAudio());
        }
    }

    private IEnumerator PlayDoorAudio()
    {
        isPlaying = true;

        audioSource.pitch = audioPitch;
        audioSource.PlayOneShot(doorSound);

        // Wait for sound to finish
        yield return new WaitForSeconds(doorSound.length / audioPitch);

        isPlaying = false;
    }
}