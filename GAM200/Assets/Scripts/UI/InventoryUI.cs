using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryPanel;      
    public Image inventoryIconImage;

    private bool isOpen = false;

    void Update()
    {
        // Toggle Inventory with I
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    private void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        isOpen = true;
    }

    private void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        isOpen = false;
    }
}
