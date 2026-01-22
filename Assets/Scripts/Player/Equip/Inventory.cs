using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySave
    {
        public List<WeaponInstance.Save> weapons = new();
    }

    public InventorySave ToSave()
    {
        var s = new InventorySave();
        foreach (var w in weapons)
            if (w != null) s.weapons.Add(w.ToSave());
        return s;
    }

    public void LoadFrom(InventorySave s, WeaponDatabase db)
    {
        IsRestoring = true;  // <— bắt đầu khôi phục

        weapons.Clear();
        if (s == null) { OnChanged?.Invoke(); IsRestoring = false; return; }

        foreach (var ws in s.weapons)
        {
            var inst = WeaponInstance.FromSave(ws, db.GetByKey);
            if (inst != null) weapons.Add(inst);
            else Debug.LogWarning($"[Inventory.LoadFrom] templateKey='{ws.templateKey}' không map được.");
        }

        IsRestoring = false; // <— kết thúc khôi phục
        OnChanged?.Invoke();
    }
    public bool IsRestoring { get; private set; }
    public static Inventory Instance { get; private set; }
    public event Action OnChanged;
    public List<WeaponInstance> weapons = new();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]

    static void ResetStatics() { Instance = null; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Đã có 1 Inventory khác rồi -> hủy bản trùng
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // chuyển sang scene DontDestroyOnLoad
       
    }

    public void AddWeapon(WeaponInstance inst)
    {
        if (inst == null) return;
        if (IsRestoring) return;

        const int MAX_PLUS = 10;

        // 🔍 1. Tìm món cùng template & cùng phẩm để gộp
        var match = weapons.Find(w =>
            w != null &&
            w.template == inst.template &&
            w.rarity == inst.rarity &&
            w.upgradeLevel < MAX_PLUS);

        if (match != null)
        {
            weapons.Add(inst);
            OnChanged?.Invoke();
            return;
        }

        // 🔹 2. Không có món để gộp → thêm vào túi
        if (!weapons.Contains(inst))
            weapons.Add(inst);

        OnChanged?.Invoke();
    }
    [Header("Upgrade Stones")]
    public int stone0to5 = 0;
    public int stone5to10 = 0;
    public void AddUpgradeStone(UpgradeStoneType type, int amount)
    {
        if (amount <= 0) return;

        if (type == UpgradeStoneType.Stone_0_5) stone0to5 += amount;
        else stone5to10 += amount;

        OnChanged?.Invoke();
    }

    private WeaponRarity GetNextRarity(WeaponRarity current)
    {
        switch (current)
        {
            case WeaponRarity.Common: return WeaponRarity.Rare;
            case WeaponRarity.Rare: return WeaponRarity.Epic;
            case WeaponRarity.Epic: return WeaponRarity.Legendary;
            case WeaponRarity.Legendary: return WeaponRarity.Mythic; // nếu bạn có thêm cấp
            default: return current;
        }
    }

    public bool RemoveWeapon(WeaponInstance inst)
    {
        if (weapons.Contains(inst))
        {
            weapons.Remove(inst);
            
            OnChanged?.Invoke();   // 🔥 gọi UI update ngay
            return true;
        }
        else
        {
           
            return false;
        }
    }
    public int GetStoneCount(UpgradeStoneType type)
    {
        return type == UpgradeStoneType.Stone_0_5 ? stone0to5 : stone5to10;
    }

    public bool ConsumeUpgradeStone(UpgradeStoneType type, int amount = 1)
    {
        if (amount <= 0) return true;

        if (type == UpgradeStoneType.Stone_0_5)
        {
            if (stone0to5 < amount) return false;
            stone0to5 -= amount;
        }
        else
        {
            if (stone5to10 < amount) return false;
            stone5to10 -= amount;
        }

        OnChanged?.Invoke();
        return true;
    }

}
