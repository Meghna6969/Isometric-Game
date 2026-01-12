using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private Inventory inventory;
    private Transform itemSlotContainer;
    private Transform itemSlotTemplate;
    private List<GameObject> activeSlots = new List<GameObject>();

    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color highlightColor = new Color(0.5f, 0.7f, 1f, 0.9f);
    private int highlightedIndex = -1;

    private void Awake()
    {
        itemSlotContainer = transform.Find("itemSlotContainer");
        itemSlotTemplate = itemSlotContainer.Find("itemSlotTemplate");
    }
    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
        inventory.SetOnItemListChanged(RefereshInventoryItems);
        RefereshInventoryItems();
    }
    private void RefereshInventoryItems()
    {
        foreach(GameObject slot in activeSlots)
        {
            if(slot != null && slot != itemSlotTemplate.gameObject)
            {
                Destroy(slot);
            }
        }
        activeSlots.Clear();

        int x = 0;
        int y = 0;
        float itemSlotCellSize = 70f;

        List<Item> items = inventory.GetItemList();

       for(int i = 0; i < items.Count; i++)
        {
            Item item = items[i];
            RectTransform itemSlotRectTransform = Instantiate(itemSlotTemplate, itemSlotContainer).GetComponent<RectTransform>();
            itemSlotRectTransform.gameObject.SetActive(true);
            itemSlotRectTransform.anchoredPosition = new Vector2(x * itemSlotCellSize, -y * itemSlotCellSize);

            Image background = itemSlotRectTransform.GetComponent<Image>();
            if(background != null)
            {
                background.color = (i == highlightedIndex) ? highlightColor : normalColor;
            }

            Image image = itemSlotRectTransform.Find("image").GetComponent<Image>();
            if(image != null)
            {
                image.sprite = item.GetSprite();
                image.gameObject.SetActive(true);
            }
            TextMeshProUGUI numberText = itemSlotRectTransform.Find("numberText")?.GetComponent<TextMeshProUGUI>();
            if(numberText != null)
            {
                numberText.text = (i + 1).ToString();
            }
        }
    }
}
