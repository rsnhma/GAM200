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
    public float noiseRadius = 8f;  // how far it alerts MainEnemy

    // Update is called once per frame
    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!playerInside)
            {
                EnterLocker();
            }
            else
            {
                ExitLocker();
            }
        }
    }

    void EnterLocker()
    {
        playerInside = true;
        IsPlayerInsideLocker = true;

        // Player movement disabled
        player.GetComponent<CharacterMovement>().enabled = false;

        // Player Sprite
        player.GetComponent<SpriteRenderer>().enabled = false;

        PlayLockerAudio();
        EmitNoise();
    }

    void ExitLocker()
    {
        playerInside = false;
        IsPlayerInsideLocker = false;
        // Player movement enabled
        player.GetComponent<CharacterMovement>().enabled = true;

        // Player Sprite
        player.GetComponent<SpriteRenderer>().enabled = true;

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

            interactText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            interactText.gameObject.SetActive(false);
        }
    }
}
