using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIController : MonoBehaviour
{
    [Header("Refs")]
    public GameObject shopPanel;
    public Button closeButton;
    public WeaponDatabase weaponDatabase;
    public int itemsCount = 12;
    public static bool IsShopOpen { get; private set; }



    [Header("Slots (auto if empty)")]
    public List<ShopSlotUI> slots = new List<ShopSlotUI>();

    private readonly List<(WeaponInstance inst, int price)> currentItems = new();

    void Awake()
    {
        if (shopPanel) shopPanel.SetActive(false);

        if (closeButton)
            closeButton.onClick.AddListener(Close);

        if (slots == null || slots.Count == 0)
        {
            slots = new List<ShopSlotUI>(GetComponentsInChildren<ShopSlotUI>(true));
            slots.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        }
    }

    public void Open()
    {
        IsShopOpen = true;

        if (shopPanel) shopPanel.SetActive(true);

        var mgr = FindAnyObjectByType<Manager>();
        if (mgr != null)
        {
            // F mở shop + túi
            if (mgr.panelBag) mgr.panelBag.SetActive(true);

            // Khi shop mở: không cho stats/equip chồng lên
            if (mgr.statsPanel) mgr.statsPanel.SetActive(false);
            if (mgr.panelEquipment) mgr.panelEquipment.SetActive(false);

            if (mgr.over) mgr.over.SetActive(false);
            if (mgr.character) mgr.character.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;

        GenerateShopItems();
        RefreshUI();
    }

    public void Close()
    {
        IsShopOpen = false;

        if (shopPanel) shopPanel.SetActive(false);

        var mgr = FindAnyObjectByType<Manager>();
        if (mgr != null)
        {
            // Đóng shop: đóng luôn túi (vì F mở shop + túi)
            if (mgr.panelBag) mgr.panelBag.SetActive(false);

            // trả gameplay bình thường
            if (mgr.over) mgr.over.SetActive(true);
            if (mgr.character) mgr.character.SetActive(true);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }

    public void GenerateShopItems()
    {
        currentItems.Clear();

        if (weaponDatabase == null || weaponDatabase.all == null || weaponDatabase.all.Count == 0)
        {
            Debug.LogError("[ShopUIController] weaponDatabase rỗng!");
            return;
        }

        int count = Mathf.Min(itemsCount, slots.Count);

        for (int i = 0; i < count; i++)
        {
            var tpl = weaponDatabase.all[Random.Range(0, weaponDatabase.all.Count)];
            if (tpl == null) { i--; continue; }

            var rarity = RollRarity();
            var inst = new WeaponInstance(tpl, rarity, 0);

            ApplyShopRandomStats(inst);
            int price = GetPriceByRarity(rarity);

            currentItems.Add((inst, price));
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] != null) slots[i].Clear();

        for (int i = 0; i < currentItems.Count && i < slots.Count; i++)
            slots[i].Set(currentItems[i].inst, currentItems[i].price, OnClickBuy);
    }

    private void OnClickBuy(WeaponInstance inst, int price)
    {
        if (inst == null) return;

        // ✅ PlayerWallet của bạn: dùng Instance + SpendCoin()
        var wallet = PlayerWallet.Instance;
        if (wallet == null)
        {
            Debug.LogError("[Shop] PlayerWallet.Instance = null (bạn chưa có PlayerWallet trong scene?)");
            return;
        }

        if (!wallet.SpendCoin(price))
        {
            Debug.Log("[Shop] Không đủ coin!");
            return;
        }

        // ✅ Add vào Inventory
        if (Inventory.Instance == null)
        {
            Debug.LogError("[Shop] Inventory.Instance = null (scene chưa có Inventory?)");
            return;
        }

        Inventory.Instance.AddWeapon(inst);

        // ✅ ép refresh UI nếu panel túi đang mở / hoặc UI không bắt event
        InventoryUI.Instance?.RefreshUI();

        Debug.Log($"[Shop] MUA OK: {inst.template.weaponName} | {inst.rarity} | giá {price} | túi hiện có {Inventory.Instance.weapons.Count} món");
    }

    // ===== RARITY + PRICE =====
    private WeaponRarity RollRarity()
    {
        int roll = Random.Range(0, 100);
        if (roll < 55) return WeaponRarity.Common;
        if (roll < 80) return WeaponRarity.Rare;
        if (roll < 93) return WeaponRarity.Epic;
        if (roll < 99) return WeaponRarity.Legendary;
        return WeaponRarity.Mythic;
    }

    private int GetPriceByRarity(WeaponRarity rarity)
    {
        switch (rarity)
        {
            case WeaponRarity.Common: return Random.Range(5, 11);
            case WeaponRarity.Rare: return Random.Range(12, 101);
            case WeaponRarity.Epic: return Random.Range(102, 1001);
            case WeaponRarity.Legendary: return Random.Range(1002, 9999);
            case WeaponRarity.Mythic: return 10000;
            default: return 10;
        }
    }

    private void ApplyShopRandomStats(WeaponInstance inst)
    {
        switch (inst.rarity)
        {
            case WeaponRarity.Common:
                inst.damage = Random.Range(inst.template.baseDamage, inst.template.baseDamage + 5);
                inst.strength = Random.Range(1, 6);
                inst.agility = Random.Range(1, 6);
                inst.vitality = Random.Range(1, 6);
                inst.energy = Random.Range(1, 6);
                break;

            case WeaponRarity.Rare:
                inst.damage = Random.Range(inst.template.baseDamage + 3, inst.template.baseDamage + 12);
                inst.strength = Random.Range(3, 12);
                inst.agility = Random.Range(3, 12);
                inst.vitality = Random.Range(3, 12);
                inst.energy = Random.Range(3, 12);
                break;

            case WeaponRarity.Epic:
                inst.damage = Random.Range(inst.template.baseDamage + 10, inst.template.baseDamage + 35);
                inst.strength = Random.Range(8, 25);
                inst.agility = Random.Range(8, 25);
                inst.vitality = Random.Range(8, 25);
                inst.energy = Random.Range(8, 25);
                inst.energy = Random.Range(8, 25);
                break;

            case WeaponRarity.Legendary:
                inst.damage = Random.Range(inst.template.baseDamage + 25, inst.template.baseDamage + 70);
                inst.strength = Random.Range(15, 40);
                inst.agility = Random.Range(15, 40);
                inst.vitality = Random.Range(15, 40);
                inst.energy = Random.Range(15, 40);
                break;

            case WeaponRarity.Mythic:
                inst.damage = Random.Range(inst.template.baseDamage + 60, inst.template.baseDamage + 140);
                inst.strength = Random.Range(30, 70);
                inst.agility = Random.Range(30, 70);
                inst.vitality = Random.Range(30, 70);
                inst.energy = Random.Range(30, 70);
                break;
        }
    }
}
    