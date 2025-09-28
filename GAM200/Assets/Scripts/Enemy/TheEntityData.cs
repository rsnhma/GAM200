using UnityEngine;

[CreateAssetMenu(fileName = "TheEntityData", menuName = "Scriptable Objects/TheEntityData")]
public class TheEntityData : EnemyData
{
    [Header("Movement & Detection")]
    public float captureRange = 1.5f;

    [Header("QTE Settings")]
    public float qteDuration = 5f;
    public float successPauseTime = 3f;

    [Header("Sanity")]
    public float sanityLossOnSuccess = 0.10f;
    public float sanityLossOnFail = 0.20f;
}
