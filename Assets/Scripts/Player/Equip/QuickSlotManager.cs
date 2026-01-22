// QuickSlotManager.cs
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuickSlotManager : MonoBehaviour
{
    [System.Serializable]
    public class QuickSlot
    {
        public ConsumableData itemData;
        public int quantity;
        public Image slotIcon;
        public TextMeshProUGUI quantityText;
    }

    public QuickSlot[] quickSlots;
    private HealthPlayer playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<HealthPlayer>();
        
    }

    void Start()
    {
        UpdateAllSlotsUI();
    }

    public bool AddConsumable(ConsumableData itemToAdd)
    {
        if (itemToAdd == null)
        {
            Debug.LogError("ConsumableData null khi gọi AddConsumable!");
            return false;
        }
        if (quickSlots == null || quickSlots.Length == 0)
        {
            Debug.LogError("QuickSlotManager chưa gán quickSlotImages!");
            return false;
        }
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (quickSlots[i].itemData == itemToAdd)
            {
                quickSlots[i].quantity++;
                UpdateSlotUI(i);
                return true;
            }
        }

        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (quickSlots[i].itemData == null)
            {
                quickSlots[i].itemData = itemToAdd;
                quickSlots[i].quantity = 1;
                UpdateSlotUI(i);
                return true;
            }
        }

        Debug.Log("Không còn ô trống để chứa " + itemToAdd.itemName);
        return false;
    }
    public void ClearSlots()
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].itemData = null;
            quickSlots[i].quantity = 0;
            UpdateSlotUI(i);
        }
    }
    public void UseItemInSlot(int slotIndex)
    {
        Debug.Log($"[QuickSlotManager] Bấm slot {slotIndex}");
        if (slotIndex < 0 || slotIndex >= quickSlots.Length) return;

        QuickSlot slot = quickSlots[slotIndex];
        if (slot.itemData != null && slot.quantity > 0)
        {
            Debug.Log("Sử dụng: " + slot.itemData.itemName);
            if (slot.itemData.type == ConsumableType.HealthPotion)
            {
                playerHealth.RestoreHealth(slot.itemData.restoreAmount);
            }
            else if (slot.itemData.type == ConsumableType.ManaPotion)
            {
                playerHealth.RestoreMana(slot.itemData.restoreAmount);
            }
            slot.quantity--;

            if (slot.quantity <= 0)
            {
                slot.itemData = null;
            }
            UpdateSlotUI(slotIndex);
        }
    }

    public void UpdateSlotUI(int slotIndex)
    {
        QuickSlot slot = quickSlots[slotIndex];
        if (slot.itemData != null)
        {
            slot.slotIcon.sprite = slot.itemData.itemIcon;
            slot.slotIcon.enabled = true;
            slot.quantityText.text = slot.quantity.ToString();
        }
        else
        {
            slot.slotIcon.sprite = null;
            slot.slotIcon.enabled = false;
            slot.quantityText.text = "";
        }
    }

    private void UpdateAllSlotsUI()
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            UpdateSlotUI(i);
        }
    }
}