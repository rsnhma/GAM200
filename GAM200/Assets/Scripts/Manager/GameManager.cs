using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Collectible map;
    public Collectible vhsTape;
    public Collectible collectible3;
    public Collectible collectible4;

    public void GameOver()
    {
        InventorySystem.Instance.ClearAll();

        vhsTape.ResetCollectible();
        collectible3.ResetCollectible();
        collectible4.ResetCollectible();

        foreach (Transform child in JournalManager.Instance.itemsContentParent)
            Destroy(child.gameObject);
    }
}
