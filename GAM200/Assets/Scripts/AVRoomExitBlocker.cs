using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AVRoomExitBlocker : MonoBehaviour
{
    [Header("Exit Blocker Settings")]
    [Tooltip("Dialogue ID when player tries to leave too early")]
    public string cantLeaveDialogueID = "av_room_cant_leave";

    [Tooltip("The physical barricade collider (non-trigger) that blocks player")]
    public Collider2D barricadeCollider;

    [Header("Debugging")]
    public bool showDebugLogs = true;

    private bool playerAtExit = false;

    private void Start()
    {
        // Make sure barricade is enabled at start (blocking the exit)
        if (barricadeCollider != null)
        {
            barricadeCollider.enabled = true;

            if (showDebugLogs)
            {
                Debug.Log("AV Room exit barricade enabled");
            }
        }
    }

    private void Update()
    {
        // Check current state and update barricade
        UpdateBarricadeState();

        // If player is at exit and tries to interact
        if (playerAtExit && Input.GetKeyDown(KeyCode.E))
        {
            bool canLeave = CanLeaveAVRoom();

            if (!canLeave)
            {
                if (showDebugLogs)
                {
                    Debug.Log("Blocking exit - enemy hasn't spawned yet");
                }

                // Show dialogue
                if (!string.IsNullOrEmpty(cantLeaveDialogueID))
                {
                    DialogueManager.Instance?.StartDialogueSequence(cantLeaveDialogueID);
                }
            }
            else if (showDebugLogs)
            {
                Debug.Log("Exit allowed - enemy has spawned");
            }
        }
    }

    private void UpdateBarricadeState()
    {
        if (barricadeCollider == null) return;

        bool canLeave = CanLeaveAVRoom();

        // Enable barricade if player can't leave, disable if they can
        if (barricadeCollider.enabled && canLeave)
        {
            // Remove the barricade - player can now leave
            barricadeCollider.enabled = false;

            if (showDebugLogs)
            {
                Debug.Log("AV Room exit barricade removed!");
            }
        }
        else if (!barricadeCollider.enabled && !canLeave)
        {
            // Re-enable barricade (shouldn't normally happen, but just in case)
            barricadeCollider.enabled = true;

            if (showDebugLogs)
            {
                Debug.Log("AV Room exit barricade re-enabled");
            }
        }
    }

    private bool CanLeaveAVRoom()
    {
        if (AVRoomController.Instance != null)
        {
            return AVRoomController.Instance.CanLeaveAVRoom();
        }

        if (showDebugLogs)
        {
            Debug.LogWarning("AVRoomController not found! Allowing exit as fallback.");
        }
        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtExit = true;

            if (showDebugLogs)
            {
                Debug.Log("Player at AV Room exit trigger");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerAtExit = false;

            if (showDebugLogs)
            {
                Debug.Log("Player left AV Room exit trigger");
            }
        }
    }
}