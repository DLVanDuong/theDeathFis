using UnityEngine;

[CreateAssetMenu(fileName = "Nhẫn/Ring", menuName = "Equipment/RingData")]
public class RingData : EquipmentData
{
    [Header("Thuộc tính nhẫn")]
    public string ringName; // Tên của nhẫn
    public float attackPower = 5f; // Sát thương tăng cơ bản của nhẫn
}
