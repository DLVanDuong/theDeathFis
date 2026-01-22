using UnityEngine;

public enum EquipmentSlot { 
    RightHand,// Vũ khí chính
    LeftHand,// Vũ khí phụ
    Head,// Mũ
    Body,// Áo giáp
    Hands,// Găng tay
    Legs,// Quần
    Feet,// Giày
    Ring1,// Nhẫn 1
    Ring2// Nhẫn 2
}


public class EquipmentData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string weaponName;       // 👉 Tên hiển thị của trang bị
    public Sprite icon;           // 👉 Icon hiển thị trong inventory

    [Header("Trang bị")]
    public EquipmentSlot slot;

    [Tooltip("Prefab (mô hình 3D) của trang bị sẽ được hiển thị trên nhân vật khi mặc.")]
    public GameObject equipPrefab;

    [Tooltip("ID của loại vũ khí. 0: Unarmed, 1: Sword, 2: Axe, 3: Bow,.....")]
    public int weaponTypeID;

    [Tooltip("Đánh dấu nếu đây là vũ khí cầm 2 tay")]
    public bool isTwoHanded = false;
}
