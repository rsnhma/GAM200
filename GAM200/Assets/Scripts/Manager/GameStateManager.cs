using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    [Header("Game Progress Flags")]
    public bool hasEnteredAVRoom = false;
    public bool hasFoundAVRoom = false;
    public bool hasEnteredCourtyard = false;
    public bool hasFoundCourtyard = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this when player enters AV room for the first time
    public void OnAVRoomEntered()
    {
        if (!hasEnteredAVRoom)
        {
            hasEnteredAVRoom = true;
            hasFoundAVRoom = true;
            Debug.Log("Player has entered AV Room for the first time!");
        }
    }

    // Call this when player enters Courtyard for the first time
    public void OnCourtyardEntered()
    {
        if (!hasEnteredCourtyard)
        {
            hasEnteredCourtyard = true;
            hasFoundCourtyard = true;
            Debug.Log("Player has entered Courtyard for the first time!");
        }
    }

    // Reset game state 
    public void ResetGameState()
    {
        hasEnteredAVRoom = false;
        hasFoundAVRoom = false;
        hasEnteredCourtyard = false;
        hasFoundCourtyard = false;
    }
}