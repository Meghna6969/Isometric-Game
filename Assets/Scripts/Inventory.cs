using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<Item> itemList;
    private System.Action onItemListChanged;

    public Inventory()
    {
        itemList = new List<Item>();
    }
    public void SetOnItemListChanged(System.Action callback)
    {
        onItemListChanged = callback;
    }
    public void AddItem(Item newItem)
    {
        if (newItem.isStackable())
        {
            bool itemAlreadyInInventory = false;
            foreach(Item inventoryItem in itemList)
            {
                if(inventoryItem.itemType == newItem.itemType)
                {
                    inventoryItem.objectInstances.AddRange(newItem.objectInstances);
                    inventoryItem.physicsColliders.AddRange(newItem.physicsColliders);
                    inventoryItem.triggerColliders.AddRange(newItem.triggerColliders);
                    itemAlreadyInInventory = true;
                    break;
                }
            }
            if (!itemAlreadyInInventory)
            {
                itemList.Add(newItem);
            }
        }
        else
        {
            itemList.Add(newItem);
        }
        onItemListChanged?.Invoke();
    }
    public void RemoveOneFromStack(Item item)
    {
      if(item.objectInstances.Count > 0)
        {
            item.objectInstances.RemoveAt(0);
            item.physicsColliders.RemoveAt(0);
            item.triggerColliders.RemoveAt(0);
        }
        if(item.amount <= 0)
        {
            itemList.Remove(item);
        }
        onItemListChanged?.Invoke();
    }
    
    public bool HasItem(Item.ItemType itemType)
    {
        foreach(Item item in itemList)
        {
            if(item.itemType == itemType)
            {
                return true;
            }
        }
        return false;
    }
    public int GetItemCount(Item.ItemType itemType)
    {
        foreach(Item item in itemList)
        {
            if(item.itemType == itemType)
            {
                return item.amount;
            }
        }
        return 0;
    }

    public List<Item> GetItemList()
    {
        return itemList;
    }
}
