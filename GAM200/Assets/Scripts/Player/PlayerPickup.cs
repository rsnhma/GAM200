using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    private Collectible nearbyCollectible;

    private void Update()
    {
        // Left click picks up collectible if near
        if (nearbyCollectible != null && Input.GetMouseButtonDown(0))
        {
            nearbyCollectible.Pickup();
            nearbyCollectible = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Collectible collectible = other.GetComponent<Collectible>();
        if (collectible != null)
        {
            nearbyCollectible = collectible;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Collectible collectible = other.GetComponent<Collectible>();
        if (collectible != null && collectible == nearbyCollectible)
        {
            nearbyCollectible = null;
        }
    }
}
