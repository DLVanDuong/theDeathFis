using UnityEngine;

public enum ConsumableType
{
    HealthPotion,
    ManaPotion
}

[CreateAssetMenu(fileName = "Vật phẩm/Consumable", menuName = "Items/ConsumableData")]
public class ConsumableData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string itemName;
    public ConsumableType type;
    public int restoreAmount; // Lượng máu hoặc mana hồi lại

    [Header("Hiển thị")]
    public Sprite itemIcon; // Icon để hiển thị trên UI
    public GameObject pickupPrefab; // Prefab khi item rơi ra thế giới
}