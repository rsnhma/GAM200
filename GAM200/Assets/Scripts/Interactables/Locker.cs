using TMPro;
using UnityEngine;
using System.Collections;

public class Locker : MonoBehaviour
{
    private bool playerNearby = false;
    private bool playerInside = false;
    public static bool IsPlayerInsideLocker = false;
    private GameObject player;

    [SerializeField] TextMeshProUGUI interactText;

    [Header("Animation")]
    [SerializeField] private Animator lockerAnimator;
    [SerializeField] private float hidePlayerDelay = 0.3f; // Time to wait before hiding player during opening

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lockerSound;
    [Range(0.5f, 3f)]
    public float audioPitch = 2.2f; // 1 = normal, >1 = faster/higher pitch, <1 = slower/lower pitch

    [Header("Noise Settings")]
    public float noiseRadius = 8f;

    private bool isAnimating = false;
    private bool canInteract = false;

    private void Awake()
    {
        // Disable animator on start to prevent auto-play
        if (lockerAnimator != null)
        {
            lockerAnimator.enabled = false;
        }
    }

    private void Start()
    {
        // Ensure locker starts in closed state
        if (lockerAnimator != null)
        {
            lockerAnimator.enabled = true;
            lockerAnimator.Play("Locker_Close", 0, 1f); // Play at the end (fully closed)
            lockerAnimator.enabled = false; // Disable again after setting initial state
            Debug.Log("Locker initialized to closed state");
        }
        else
        {
            Debug.LogError("Locker Animator is not assigned!");
        }
    }

    void Update()
    {
        // Prevent input during animation
        if (isAnimating)
        {
            Debug.Log("Locker is animating, ignoring input");
            return;
        }

        // Allow exiting when inside (don't need to be nearby when already inside)
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player pressing E to exit locker");
            StartCoroutine(ExitLockerSequence());
        }
        // Normal entry when nearby but not inside
        else if (playerNearby && canInteract && Input.GetKeyDown(KeyCode.E) && !playerInside)
        {
            Debug.Log("Player pressing E to enter locker");
            StartCoroutine(EnterLockerSequence());
        }
    }

    IEnumerator EnterLockerSequence()
    {
        isAnimating = true;
        Debug.Log("Starting enter locker sequence");

        // Enable animator to play animation
        if (lockerAnimator != null)
        {
            lockerAnimator.enabled = true;
        }

        // Step 1: Trigger open animation
        if (lockerAnimator != null)
        {
            Debug.Log("Triggering Open animation");
            lockerAnimator.ResetTrigger("Close"); // Clear any pending close triggers
            lockerAnimator.SetTrigger("Open");

            // Wait a frame for trigger to register
            yield return null;

            // Wait until the Open animation state starts
            float timeout = 2f;
            float elapsed = 0f;
            while (!lockerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Locker_Open") && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= timeout)
            {
                Debug.LogWarning("Open animation didn't start - check Animator state name!");
            }
            else
            {
                Debug.Log("Open animation started");
                // Wait for open animation to finish
                float animLength = lockerAnimator.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(animLength);
            }
        }

        PlayLockerAudio();

        // Wait for a moment before hiding player
        yield return new WaitForSeconds(hidePlayerDelay);

        // Step 2: Hide player and disable controls
        Debug.Log("Hiding player");
        if (player != null)
        {
            player.GetComponent<SpriteRenderer>().enabled = false;
            player.GetComponent<CharacterMovement>().enabled = false;
            player.GetComponent<Collider2D>().enabled = false;
        }

        yield return new WaitForSeconds(0.2f);

        // Step 3: Trigger close animation
        if (lockerAnimator != null)
        {
            Debug.Log("Triggering Close animation");
            lockerAnimator.ResetTrigger("Open"); // Clear any pending open triggers
            lockerAnimator.SetTrigger("Close");

            // Wait a frame for trigger to register
            yield return null;

            // Wait until the Close animation state starts
            float timeout = 2f;
            float elapsed = 0f;
            while (!lockerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Locker_Close") && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= timeout)
            {
                Debug.LogWarning("Close animation didn't start - check Animator state name!");
            }
            else
            {
                Debug.Log("Close animation started");
                // Wait for close animation to complete
                float animLength = lockerAnimator.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(animLength);
            }

            // Disable animator after animation completes
            lockerAnimator.enabled = false;
        }

        // Update state
        playerInside = true;
        IsPlayerInsideLocker = true;
        if (interactText != null)
        {
            interactText.gameObject.SetActive(false);
        }

        EmitNoise();
        isAnimating = false;
        Debug.Log("Enter sequence complete");
    }

    IEnumerator ExitLockerSequence()
    {
        isAnimating = true;

        // Enable animator to play animation
        if (lockerAnimator != null)
        {
            lockerAnimator.enabled = true;
        }

        // Step 1: Trigger open animation
        if (lockerAnimator != null)
        {
            lockerAnimator.SetTrigger("Open");

            // Wait until the Open animation state starts
            yield return new WaitUntil(() =>
                lockerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Locker_Open"));
        }

        PlayLockerAudio();

        // Wait for a moment before showing player
        yield return new WaitForSeconds(hidePlayerDelay);

        // Step 2: Show player and enable controls
        if (player != null)
        {
            player.GetComponent<SpriteRenderer>().enabled = true;
            player.GetComponent<CharacterMovement>().enabled = true;
            player.GetComponent<Collider2D>().enabled = true;
        }

        // Step 3: Trigger close animation
        if (lockerAnimator != null)
        {
            lockerAnimator.SetTrigger("Close");

            // Wait until the Close animation state starts
            yield return new WaitUntil(() =>
                lockerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Locker_Close"));

            // Wait for the close animation to complete
            float animLength = lockerAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength);

            // Disable animator after animation completes
            lockerAnimator.enabled = false;
        }

        // Update state
        playerInside = false;
        IsPlayerInsideLocker = false;

        EmitNoise();
        isAnimating = false;
    }

    private void PlayLockerAudio()
    {
        if (audioSource != null && lockerSound != null)
        {
            audioSource.pitch = audioPitch;
            audioSource.PlayOneShot(lockerSound);
        }
    }

    private void EmitNoise()
    {
        if (player != null)
        {
            NoiseSystem.EmitNoise(player.transform.position, noiseRadius);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            player = other.gameObject;
            StartCoroutine(EnableInteractionNextFrame());

            // Only show interact text if player is not already inside
            if (!playerInside && interactText != null)
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
            canInteract = false;

            // Only hide interact text if player is not inside
            if (!playerInside && interactText != null)
            {
                interactText.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator EnableInteractionNextFrame()
    {
        yield return null;
        canInteract = true;
    }
}