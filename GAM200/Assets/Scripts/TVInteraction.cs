using UnityEngine;
using System.Collections;

public class TVInteraction : MonoBehaviour
{
    [Header("Enemy Spawn")]
    public MainEnemy enemyPrefab;
    public Transform spawnPoint;

    private bool hasBeenUsed = false;

    /// Called by ItemSlotUI when player drags and drops the VHS onto the TV
    public void HandleVHSUse()
    {
        if (hasBeenUsed) return;
        StartCoroutine(InsertVHS());
    }

    private IEnumerator InsertVHS()
    {
        Debug.Log("VHS inserted to TV");
        hasBeenUsed = true;

        // Optional cutscene or delay
        yield return new WaitForSeconds(2f);

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ActivateEnemy(spawnPoint.position);
            Debug.Log("Enemy spawned via EnemyManager!");
        }
        else
        {
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity).BeginChase();
            Debug.Log("Enemy spawned directly (fallback)");
        }
    }
}
