using UnityEngine;

[CreateAssetMenu(fileName = "WanderingSpiritData", menuName = "Scriptable Objects/WanderingSpiritData")]
public class WanderingSpiritData : EnemyData
{
    [Header("QTE")]
    public float qteDuration = 3f;

    [Header("Sanity / Noise")]
    public float sanityLossOnFail = 5f;
    public float noiseOnFail = 5f;
}
