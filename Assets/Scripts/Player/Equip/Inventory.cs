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

        // ===== Stones (Save/Load) =====
        public int stone0to5;
        public int stone5to10;

        // ===== Element crafting stones =====
        public int stoneElement;   // Phôi đá / Đá ngũ sắc (Stone_Element)
        public int stoneWind;
        public int stoneThunder;
        public int stoneFire;
        public int stoneEarth;
    }

    public InventorySave ToSave()
    {
        var s = new InventorySave();

        foreach (var w in weapons)
            if (w != null) s.weapons.Add(w.ToSave());

        // save stones
        s.stone0to5 = stone0to5;
        s.stone5to10 = stone5to10;

        // save element stones
        s.stoneElement = stoneElement;
        s.stoneWind = stoneWind;
        s.stoneThunder = stoneThunder;
        s.stoneFire = stoneFire;
        s.stoneEarth = stoneEarth;

        return s;
    }

    public void LoadFrom(InventorySave s, WeaponDatabase db)
    {
        IsRestoring = true;  // <— bắt đầu khôi phục

        weapons.Clear();
        if (s == null)
        {
            OnChanged?.Invoke();
            IsRestoring = false;
            return;
        }

        foreach (var ws in s.weapons)
        {
            var inst = WeaponInstance.FromSave(ws, db.GetByKey);
            if (inst != null) weapons.Add(inst);
            else Debug.LogWarning($"[Inventory.LoadFrom] templateKey='{ws.templateKey}' không map được.");
        }

        // load stones
        stone0to5 = s.stone0to5;
        stone5to10 = s.stone5to10;

        // load element stones
        stoneElement = s.stoneElement;
        stoneWind = s.stoneWind;
        stoneThunder = s.stoneThunder;
        stoneFire = s.stoneFire;
        stoneEarth = s.stoneEarth;

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
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddWeapon(WeaponInstance inst)
    {
        if (inst == null) return;
        if (IsRestoring) return;

        const int MAX_PLUS = 10;

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

        if (!weapons.Contains(inst))
            weapons.Add(inst);

        OnChanged?.Invoke();
    }

    [Header("Upgrade Stones")]
    public int stone0to5 = 0;
    public int stone5to10 = 0;

    [Header("Element Craft Stones")]
    public int stoneElement = 0; // phôi đá / ngũ sắc
    public int stoneWind = 0;
    public int stoneThunder = 0;
    public int stoneFire = 0;
    public int stoneEarth = 0;

    public void AddUpgradeStone(UpgradeStoneType type, int amount)
    {
        if (amount <= 0) return;

        switch (type)
        {
            case UpgradeStoneType.Stone_0_5:
                stone0to5 += amount;
                break;

            case UpgradeStoneType.Stone_5_10:
                stone5to10 += amount;
                break;

            case UpgradeStoneType.Stone_Element:
                stoneElement += amount;
                break;

            case UpgradeStoneType.Stone_Wind:
                stoneWind += amount;
                break;

            case UpgradeStoneType.Stone_Thunder:
                stoneThunder += amount;
                break;

            case UpgradeStoneType.Stone_Fire:
                stoneFire += amount;
                break;

            case UpgradeStoneType.Stone_Earth:
                stoneEarth += amount;
                break;

            default:
                Debug.LogWarning($"[Inventory] AddUpgradeStone: type '{type}' chưa được xử lý.");
                break;
        }

        OnChanged?.Invoke();
    }

    private WeaponRarity GetNextRarity(WeaponRarity current)
    {
        switch (current)
        {
            case WeaponRarity.Common: return WeaponRarity.Rare;
            case WeaponRarity.Rare: return WeaponRarity.Epic;
            case WeaponRarity.Epic: return WeaponRarity.Legendary;
            case WeaponRarity.Legendary: return WeaponRarity.Mythic;
            default: return current;
        }
    }

    public bool RemoveWeapon(WeaponInstance inst)
    {
        if (weapons.Contains(inst))
        {
            weapons.Remove(inst);
            OnChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int GetStoneCount(UpgradeStoneType type)
    {
        return type switch
        {
            UpgradeStoneType.Stone_0_5 => stone0to5,
            UpgradeStoneType.Stone_5_10 => stone5to10,

            UpgradeStoneType.Stone_Element => stoneElement,
            UpgradeStoneType.Stone_Wind => stoneWind,
            UpgradeStoneType.Stone_Thunder => stoneThunder,
            UpgradeStoneType.Stone_Fire => stoneFire,
            UpgradeStoneType.Stone_Earth => stoneEarth,

            _ => 0
        };
    }

    public bool ConsumeUpgradeStone(UpgradeStoneType type, int amount = 1)
    {
        if (amount <= 0) return true;

        int have = GetStoneCount(type);
        if (have < amount) return false;

        switch (type)
        {
            case UpgradeStoneType.Stone_0_5: stone0to5 -= amount; break;
            case UpgradeStoneType.Stone_5_10: stone5to10 -= amount; break;

            case UpgradeStoneType.Stone_Element: stoneElement -= amount; break;
            case UpgradeStoneType.Stone_Wind: stoneWind -= amount; break;
            case UpgradeStoneType.Stone_Thunder: stoneThunder -= amount; break;
            case UpgradeStoneType.Stone_Fire: stoneFire -= amount; break;
            case UpgradeStoneType.Stone_Earth: stoneEarth -= amount; break;
        }

        OnChanged?.Invoke();
        return true;
    }
}
