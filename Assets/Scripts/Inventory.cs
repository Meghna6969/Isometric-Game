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
            foreach(Item inventoryItem)
        }
    }
    public List<Item> GetItemList()
    {
        return itemList;
    }
}
