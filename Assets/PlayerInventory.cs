using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Analytics;
using Unity.VisualScripting;

public class PlayerInventory : MonoBehaviour
{

    [Header("Inventory Settings")]
    [SerializeField] private int maxInventroySlots = 6;

    [Header("References")]
    [SerializeField] private Transform inventoryHolder;
    [SerializeField] private PlayerPickup playerPickup;
    [SerializeField] private InventoryUI inventoryUI;
    
    [System.Serializable]
    public class InventoryItem
    {
        public GameObject gameObject;
        public Rigidbody rigidbody;
        public Collider physicsCollider;
        public Collider triggerCollider;
        public string itemName;

        public InventoryItem(GameObject obj, Collider physics, Collider trigger)
        {
            gameObject = obj;
            rigidbody = obj.GetComponent<Rigidbody>();
            physicsCollider = physics;
            triggerCollider = trigger;
            PickupObject pickupObj = obj.GetComponent<PickupObject>();
            itemName = pickupObj != null ? pickupObj.itemName : obj.name;
        }
    }

}
