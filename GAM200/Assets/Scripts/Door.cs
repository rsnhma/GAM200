using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [SerializeField] private string targetScene;      // Scene to load
    [SerializeField] private string entryPointId;     // Entry ID in target scene
    [SerializeField] private QTESystem qteSystem;     // Reference to your QTE manager

    private bool playerNearby;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("QTE Triggered at the door!");

            // Always trigger QTE, regardless of enemy
            qteSystem.BeginQTE(OnQTESuccess, OnQTEFail);
        }
    }

    private void OnQTESuccess()
    {
        Debug.Log("Door QTE Success Loading next scene");
        PlayerSpawnManager.nextEntryPointId = entryPointId;
        SceneManager.LoadScene(targetScene);
    }

    private void OnQTEFail()
    {
        Debug.Log("Door QTE Failed Player fumbled at the door!");
        // For now, nothing happens. Later: reduce sanity or trigger penalties
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }
}
