using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private HashSet<string> mainItems = new HashSet<string>();
    private HashSet<string> hiddenItems = new HashSet<string>();

    public void AddItem(string itemID, bool isHidden)
    {
        if (isHidden)
        {
            if (!hiddenItems.Contains(itemID))
            {
                hiddenItems.Add(itemID);
                Debug.Log("Collected hidden item: " + itemID);
            }
        }
        else
        {
            if (!mainItems.Contains(itemID))
            {
                mainItems.Add(itemID);
                Debug.Log("Collected main item: " + itemID);
            }
        }
    }

    public bool HasAllMainItems(List<string> requiredItems)
    {
        foreach (string id in requiredItems)
            if (!mainItems.Contains(id)) return false;
        return true;
    }

    public bool HasAllHiddenItems(List<string> requiredItems)
    {
        foreach (string id in requiredItems)
            if (!hiddenItems.Contains(id)) return false;
        return true;
    }
}
