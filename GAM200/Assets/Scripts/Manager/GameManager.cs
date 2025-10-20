using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Collectible vhsTape;
    public Collectible collectible3;
    public Collectible collectible4;

    public void GameOver()
    {
        // Clear inventory system
        InventorySystem.Instance.ClearAll();

        // Reset collectibles (only non-puzzle rewards get reset)
        vhsTape.ResetCollectible();
        collectible3.ResetCollectible();
        collectible4.ResetCollectible();

        // Clear inventory UI slots
        foreach (Transform child in JournalManager.Instance.inventoryContentParent)
            Destroy(child.gameObject);

        // Clear memorabilia UI slots (if you also want to reset memorabilia on game over)
        foreach (Transform child in JournalManager.Instance.memorabiliaContentParent)
            Destroy(child.gameObject);

        // Clear internal dictionaries in JournalManager
        JournalManager.Instance.ClearAllJournalData();
    }
}