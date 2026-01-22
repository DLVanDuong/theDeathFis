using UnityEngine;

[CreateAssetMenu(fileName = "Áo giáp/Armor", menuName = "Equipment/ArmorData")]
public class ArmorData : EquipmentData
{ 
    [Header("thuộc tính áo giáp")]
    public string ArmorName; // Tên của áo giáp
    public float defense = 10f; // Sát thương giảm cơ bản của áo giáp  
}
