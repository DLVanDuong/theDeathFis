using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;                 // kéo Icon (Image)
    public TextMeshProUGUI nameText;        // kéo Name (TMP)
    public TextMeshProUGUI priceText;       // kéo Price (TMP)
    public TextMeshProUGUI statsText;       // kéo Stats (TMP) (optional)
    public Button button;                   // Button của slot (hoặc auto)

    private WeaponInstance item;
    private int price;
    private Action<WeaponInstance, int> onBuy;

    void Awake()
    {
        if (!button) button = GetComponent<Button>();
        if (!button) button = gameObject.AddComponent<Button>();
    }

    public void Clear()
    {
        item = null;
        price = 0;
        onBuy = null;

        if (iconImage) { iconImage.sprite = null; iconImage.color = new Color(1, 1, 1, 0); }
        if (nameText) nameText.text = "";
        if (priceText) priceText.text = "";
        if (statsText) statsText.text = "";

        button.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    public void Set(WeaponInstance inst, int priceValue, Action<WeaponInstance, int> onBuyCallback)
    {
        item = inst;
        price = priceValue;
        onBuy = onBuyCallback;

        gameObject.SetActive(true);

        // ✅ ICON: lấy từ WeaponData.icon (đúng với InventoryUI của bạn)
        if (iconImage)
        {
            iconImage.sprite = inst.template.icon;
            iconImage.color = iconImage.sprite ? Color.white : new Color(1, 1, 1, 0);
        }

        // ✅ NAME + COLOR theo rarity
        if (nameText)
        {
            nameText.text = RarityDisplay.FormatDisplayName(inst.template.weaponName, inst.rarity, inst.upgradeLevel);
            nameText.color = RarityDisplay.GetRarityColor(inst.rarity);
        }

        // ✅ PRICE
        if (priceText) priceText.text = $"{price} Đồng";

        // ✅ STATS (optional)
        if (statsText)
        {
            statsText.text =
                $"DMG: {inst.damage}\n" +
                $"STR: {inst.strength}  AGI: {inst.agility}\n" +
                $"VIT: {inst.vitality}  ENE: {inst.energy}";
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onBuy?.Invoke(item, price));
    }
}
