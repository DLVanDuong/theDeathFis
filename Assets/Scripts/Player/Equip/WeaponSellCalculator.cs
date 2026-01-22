using UnityEngine;

public static class WeaponSellCalculator
{
    public static int GetSellPrice(WeaponInstance inst)
    {
        if (inst == null) return 0;

        float rarityMul = inst.rarity switch
        {
            WeaponRarity.Common => 1f,
            WeaponRarity.Rare => 1.5f,
            WeaponRarity.Epic => 2.2f,
            WeaponRarity.Legendary => 3.5f,
            WeaponRarity.Mythic => 5f,
            _ => 1f
        };

        int basePrice = inst.requiredLevel * 10;
        int upgradeBonus = inst.upgradeLevel * 15;

        int price = Mathf.RoundToInt((basePrice + upgradeBonus) * rarityMul);
        return Mathf.Max(1, price);
    }
}
