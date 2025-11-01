using TMPro;
using UnityEngine;

public class Locker : MonoBehaviour
{
    private bool playerNearby = false;
    private bool playerInside = false;
    public static bool IsPlayerInsideLocker = false;
    private GameObject player;
    [SerializeField] TextMeshProUGUI interactText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lockerSound;

    [Header("Noise Settings")]
    public float noiseRadius = 8f;

    void Update()
    {
        // Allow exiting regardless of proximity when already inside
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            ExitLocker();
        }
        // Normal entry when nearby but not inside
        else if (playerNearby && Input.GetKeyDown(KeyCode.E) && !playerInside)
        {
            EnterLocker();
        }
    }

    void EnterLocker()
    {
        playerInside = true;
        IsPlayerInsideLocker = true;

        // Player movement disabled
        player.GetComponent<CharacterMovement>().enabled = false;

        // Hide Player Sprite
        player.GetComponent<SpriteRenderer>().enabled = false;

        // Disable player collider (so enemies can't detect them)
        player.GetComponent<Collider2D>().enabled = false;

        // Hide interact text since player is inside
        interactText.gameObject.SetActive(false);

        PlayLockerAudio();
        EmitNoise();
    }

    void ExitLocker()
    {
        playerInside = false;
        IsPlayerInsideLocker = false;

        // Player movement enabled
        player.GetComponent<CharacterMovement>().enabled = true;

        // Show Player Sprite
        player.GetComponent<SpriteRenderer>().enabled = true;

        // Re-enable player collider
        player.GetComponent<Collider2D>().enabled = true;

        PlayLockerAudio();
        EmitNoise();
    }

    private void PlayLockerAudio()
    {
        if (audioSource != null && lockerSound != null)
        {
            audioSource.PlayOneShot(lockerSound);
        }
    }

    private void EmitNoise()
    {
        NoiseSystem.EmitNoise(player.transform.position, noiseRadius);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            player = other.gameObject;

            // Only show interact text if player is not already inside
            if (!playerInside)
            {
                interactText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            // Only hide interact text if player is not inside
            if (!playerInside)
            {
                interactText.gameObject.SetActive(false);
            }
        }
    }
}