using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;

    [Header("Sanity")]
    public float maxSanity = 100f;

    /*[Header("Stamina"]
    public float maxStamina = 100f;
    public float staminaDrainRate = 10f;   // per second
    public float staminaRecoveryRate = 5f; // per second*/

    [Header("Noise")]
    public float baseNoiseLevel = 1f;
    public float sprintNoiseMultiplier = 2f;
    public float interactNoise = 5f;
}
