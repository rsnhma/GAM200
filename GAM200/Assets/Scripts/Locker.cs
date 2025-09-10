using TMPro;
using UnityEngine;

public class Locker : MonoBehaviour
{
    private bool playerNearby = false;
    private bool playerInside = true;
    private GameObject player;
    [SerializeField] TextMeshProUGUI interactText;

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

        // Player movement disabled
        player.GetComponent<CharacterMovement>().enabled = false;

        // Player Sprite
        player.GetComponent<SpriteRenderer>().enabled = false;
    }

    void ExitLocker()
    {
        playerInside = false;
        // Player movement enabled
        player.GetComponent<CharacterMovement>().enabled = true;

        // Player Sprite
        player.GetComponent<SpriteRenderer>().enabled = true;
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
