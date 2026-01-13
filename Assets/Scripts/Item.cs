using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Item
{
    public enum ItemType
    {
        Squeaky,
        Clock,
    }
    public ItemType itemType;
    public List<GameObject> objectInstances = new List<GameObject>();
    public List<Collider> physicsColliders = new List<Collider>();
    public List<Collider> triggerColliders = new List<Collider>();

    public int amount => objectInstances.Count;

    public GameObject GetPrimaryObject()
    {
        if(objectInstances.Count > 0) return objectInstances[0];
        return null;
    }

    public Sprite GetSprite()
    {
        switch (itemType)
        {
            default:
            case ItemType.Squeaky: return ItemAssets.Instance.squeakySprite;
            case ItemType.Clock: return ItemAssets.Instance.clockSprite;
        }
    }
    public String GetItemName()
    {
        switch (itemType)
        {
            case ItemType.Squeaky: return "Squeaky Toy";
            case ItemType.Clock: return "Alaram Clock";
            default: return "Unknown";
        }
    }
    public bool isStackable()
    {
        return true;
    }
}
