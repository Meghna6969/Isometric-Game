using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(0.4f, 0.6f, 1f, 0.9f);

    private List<InventorySlot> slots = new List<InventorySlot>();
    private int maxSlots = 6;

    public class InventorySlot
    {
        public GameObject slotObject;
        public UnityEngine.UI.Image background;
        public TextMeshProUGUI numberText;
        public TextMeshProUGUI itemNameText;
        public UnityEngine.UI.Image itemIcon;
    }
    void Start()
    {
        CreateInventroySlots();
    }
    private void CreateInventroySlots()
    {
        foreach(Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        for(int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            InventorySlot slot = new InventorySlot();

            slot.slotObject = slotObj;
            slot.background = slotObj.GetComponent<UnityEngine.UI.Image>();

            Transform numberTransform = slotObj.transform.Find("NumberText");
            Transform nameTransform = slotObj.transform.Find("ItemNameText");
            Transform iconTransform = slotObj.transform.Find("ItemIcon");

            if(numberTransform != null)
            {
                slot.numberText = numberTransform.GetComponent<TextMeshProUGUI>();
            }
            if(numberTransform != null)
            {
                slot.itemNameText = nameTransform.GetComponent<TextMeshProUGUI>();
            }
            if(iconTransform != null)
            {
                slot.itemIcon = iconTransform.GetComponent<UnityEngine.UI.Image>();
            }

            if(slot.numberText != null)
            {
                slot.numberText.text = (i + 1).ToString();
            }
            if(slot.itemNameText != null)
            {
                slot.itemNameText.text = "Empty";
                slot.itemNameText.color = new Color(1, 1, 1, 0.3f);
            }

            if(slot.itemIcon != null)
            {
                slot.itemIcon.enabled = false;
            }
            if(slot.background != null)
            {
                slot.background.color = normalColor;
            }
            slots.Add(slot);
        }
         
    }
    public void UpdateInventoryDisplay(List<PlayerInventory.InventoryItem> inventory)
    {
        for(int i = 0; i < slots.Count; i++)
        {
            if(i < inventory.Count)
            {
                InventorySlot slot = slots[i];
                PlayerInventory.InventoryItem item = inventory[i];

                if(slot.itemNameText != null)
                {
                    slot.itemNameText.text = item.itemName;
                    slot.itemNameText.color = Color.white;
                }

                if(slot.itemIcon != null)
                {
                    slot.itemIcon.enabled = true;
                }
            }
            else
            {
                InventorySlot slot = slots[i];

                if(slot.itemNameText != null)
                {
                    slot.itemNameText.text = "Empty";
                }
            }
        }
    }
}
