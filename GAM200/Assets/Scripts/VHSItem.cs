using UnityEngine;
using TMPro;

public class VHSItem : MonoBehaviour
{
    private bool playerNearby = false;
    public static bool hasVHS = false;

    [SerializeField] private TextMeshProUGUI interactText;

    private void Update()
    {
        if (playerNearby && Input.GetMouseButtonDown(0))
        {
            hasVHS = true;
            interactText.gameObject.SetActive(false);
            gameObject.SetActive(false); // tape disappears
            Debug.Log("Player picked up VHS tape");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            interactText.text = "Left Click to pick up VHS";
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
