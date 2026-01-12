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
    public void AddItem(Item item)
    {
        if (item.isStackable())
        {
            bool itemAlreadyInInventory = false;
            foreach(Item inventoryItem in itemList)
            {
                if(inventoryItem.itemType == item.itemType)
                {
                    inventoryItem.amount += item.amount;
                    itemAlreadyInInventory = true;
                    break;
                }
            }
            if (!itemAlreadyInInventory)
            {
                itemList.Add(item);
            }
        }
        else
        {
            itemList.Add(item);
        }
        onItemListChanged?.Invoke();
    }
    public void RemoveItem(Item item)
    {
        if (item.isStackable())
        {
            foreach(Item inventoryItem in itemList)
            {
                if(inventoryItem.itemType == item.itemType)
                {
                    inventoryItem.amount -= item.amount;
                    if(inventoryItem.amount <= 0)
                    {
                        itemList.Remove(inventoryItem);
                    }
                    break;
                }
            }
        }
        else
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
