using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    [Header("Refs")]
    public EquipmentManager equipmentManager;

    [Header("Slot icons (kéo Image vào)")]
    public Image rightHand;
    public Image leftHand;
    public Image head;
    public Image body;
    public Image hands;
    public Image legs;
    public Image feet;
    public Image ring1;   // nhẫn phải
    public Image ring2;   // nhẫn trái

    [Header("Màu khi trống/đầy")]
    public Color emptyColor = new Color(1, 1, 1, 0.25f);
    public Color filledColor = Color.white;

    void OnEnable()
    {
        if (equipmentManager == null)
            equipmentManager = FindAnyObjectByType<EquipmentManager>();
        if (equipmentManager != null)
            equipmentManager.EquipmentChanged += Refresh;

        Refresh(); // vẽ lần đầu
    }

    void OnDisable()
    {
        if (equipmentManager != null)
            equipmentManager.EquipmentChanged -= Refresh;
    }

    public void Refresh()
    {
        if (equipmentManager == null) return;

        SetSlot(EquipmentSlot.RightHand, rightHand);
        SetSlot(EquipmentSlot.LeftHand, leftHand);
        SetSlot(EquipmentSlot.Head, head);
        SetSlot(EquipmentSlot.Body, body);
        SetSlot(EquipmentSlot.Hands, hands);
        SetSlot(EquipmentSlot.Legs, legs);
        SetSlot(EquipmentSlot.Feet, feet);
        SetSlot(EquipmentSlot.Ring1, ring1);
        SetSlot(EquipmentSlot.Ring2, ring2);
    }

    void SetSlot(EquipmentSlot slot, Image img)
    {
        if (img == null) return;

        if (equipmentManager.equippedItems.TryGetValue(slot, out EquipmentData data) && data != null)
        {
            img.sprite = data.icon;   // icon trên EquipmentData
            img.color = filledColor;
        }
        else
        {
            img.sprite = null;
            img.color = emptyColor;
        }
    }
}
