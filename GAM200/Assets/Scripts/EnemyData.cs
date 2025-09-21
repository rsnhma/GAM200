using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("General")]
    public float patrolSpeed = 1f;
    public float lineOfSightRange = 6f;
    public float chaseSpeed = 4f;
    public float chaseBreakTime = 10f;
}
