using TMPro;
using UnityEngine;

public class LootPlusLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText; // kéo TMP vào đây

    public void SetWeapon(WeaponInstance inst)
    {
        if (nameText == null || inst == null || inst.template == null) return;

        string baseName = inst.template.weaponName;   // <-- template (đúng với class bạn)
        int up = inst.upgradeLevel;
        var rarity = inst.rarity;

        string plus = up > 0 ? $" +{up}" : "";
        string rarityName = RarityDisplay.GetRarityName(rarity);
        nameText.text = $"{baseName}{plus} [{rarityName}]";

        nameText.color = RarityDisplay.GetRarityColor(rarity);
    }
}
