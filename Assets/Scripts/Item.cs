using System;
using Unity.VisualScripting;
using UnityEngine;

public class Item
{
    public enum ItemType
    {
        Squeaky,
    }
    public ItemType itemType;
    public int amount;
    public GameObject gameObject;
    public Collider physicsCollider;
    public Collider triggerCollider;

    public Sprite GetSprite()
    {
        switch (itemType)
        {
            default:
            case ItemType.Squeaky: return ItemAssets.Instance.squeakySprite;
        }
    }
    public String GetItemName()
    {
        switch (itemType)
        {
            case ItemType.Squeaky: return "Squeaky Toy";
            default: return "Unknown";
        }
    }
    public bool isStackable()
    {
        return itemType == ItemType.Squeaky;
    }
}
