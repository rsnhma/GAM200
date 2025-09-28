using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    private HashSet<string> items = new HashSet<string>();
    private Dictionary<string, Action> useActions = new Dictionary<string, Action>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(string itemID, Action useAction = null)
    {
        items.Add(itemID);
        if (useAction != null) useActions[itemID] = useAction;
    }

    public bool HasItem(string itemID)
    {
        return items.Contains(itemID);
    }

    public void UseItem(string itemID)
    {
        if (items.Contains(itemID) && useActions.ContainsKey(itemID))
        {
            useActions[itemID]?.Invoke();
            items.Remove(itemID);        // remove item after use
            useActions.Remove(itemID);   // clean up action
        }
    }

    public void RemoveItem(string itemID)
    {
        items.Remove(itemID);
        useActions.Remove(itemID);
    }

    public List<string> GetAllItems()
    {
        return new List<string>(items);
    }

    public void ClearAll()
    {
        items.Clear();
        useActions.Clear();
    }
}
