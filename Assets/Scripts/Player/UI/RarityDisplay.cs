// Assets/Scripts/UI/RarityDisplay.cs (ví dụ đặt ở đây)
using UnityEngine;

public static class RarityDisplay
{
    public static string GetRarityName(WeaponRarity r) => r switch
    {
        WeaponRarity.Common => "Thường",
        WeaponRarity.Rare => "Hiếm",
        WeaponRarity.Epic => "Sử thi",
        WeaponRarity.Legendary => "Huyền thoại",
        _ => r.ToString()
    };

    public static Color GetRarityColor(WeaponRarity r) => r switch
    {
        WeaponRarity.Common => new Color(0.85f, 0.85f, 0.85f),
        WeaponRarity.Rare => new Color(0.25f, 0.55f, 1.00f),
        WeaponRarity.Epic => new Color(0.65f, 0.30f, 0.85f),
        WeaponRarity.Legendary => new Color(1.00f, 0.70f, 0.10f),
        WeaponRarity.Mythic => new Color(1.00f, 0.30f, 0.20f),
        _ => Color.white
    };

    public static string FormatDisplayName(string baseName, WeaponRarity rarity, int upgradeLevel = 0)
    {
        string plus = upgradeLevel > 0 ? $" +{upgradeLevel}" : "";
        return $"{baseName}{plus} [{GetRarityName(rarity)}]";
    }
}
