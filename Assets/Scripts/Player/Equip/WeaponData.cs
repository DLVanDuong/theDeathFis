using UnityEngine;
public enum WeaponRarity
{
    Common,     // Thường
    Rare,       // Hiếm
    Epic,       // Sử thi
    Legendary,   // Tuyệt phẩm
    Mythic      // Huyền thoại

}
public enum ArrowType { None, Normal, Fire, Ice, Lightning }
[CreateAssetMenu(fileName = "Vũ khí/Weapon", menuName = "Equipment/WeaponData")]
public class WeaponData : EquipmentData
{ 
    [Header("Phẩm chất vũ khí")]
    public WeaponRarity rarity;

    [Header("Yêu cầu")]
    public int requiredLevel = 1; // Cấp độ yêu cầu để sử dụng vũ khí

    [Header("Lưu trữ")]
    public string saveKey;

    [Header("Bow / Quiver Settings")]
    public GameObject arrowPrefabOverride;

    [Header("Bow/Quiver Link")]
    public ArrowType arrowType = ArrowType.None;

    [Header("Kỹ năng riêng")]
    public SkillData skill1; // Kỹ năng đặc biệt của vũ khí
    public SkillData skill2; // Kỹ năng đặc biệt của vũ khí

    [Header("Chỉ số mặc định (template)")]
    public int baseDamage = 10;
    public int baseStrength = 5;
    public int baseAgility = 5;
    public int baseVitality = 5;
    public int baseEnergy = 5;

    [Header("Prefab hiển thị")]
    public GameObject pickupPrefab;  // Prefab rớt ra để nhặt (có WeaponPickup.cs)
#if UNITY_EDITOR
    [ContextMenu("Generate Save Key (GUID)")]
    private void GenerateSaveKey()
    {
        if (string.IsNullOrEmpty(saveKey))
        {
            saveKey = System.Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[WeaponData] Set saveKey for '{weaponName}' = {saveKey}");
        }
    }
#endif
}