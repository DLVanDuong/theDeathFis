using System;
using System.Collections.Generic;
using UnityEngine;

using static QuickSlotManager;

[RequireComponent(typeof(AnimatorManager))]
public class EquipmentManager : MonoBehaviour
{
    [System.Serializable]
    public class EquippedSave
    {
        public List<string> slots = new();       // ví dụ "RightHand"
        public List<string> instanceIds = new(); // instanceId tương ứng
    }

    public EquippedSave ToSaveEquipped()
    {
        var s = new EquippedSave();
        foreach (var kv in GetAllEquippedInstances())
        {
            s.slots.Add(kv.Key.ToString());
            s.instanceIds.Add(kv.Value.instanceId);
        }
        return s;
    }

    public void LoadEquippedFrom(EquippedSave s, Inventory inv)
    {
        UnequipAll();
        if (s == null) return;

        for (int i = 0; i < s.slots.Count; i++)
        {
            if (System.Enum.TryParse(s.slots[i], out EquipmentSlot slot))
            {
                var inst = inv.weapons.Find(w => w.instanceId == s.instanceIds[i]);
                if (inst != null) EquipWeaponInstance(slot, inst); // dùng đúng instance, KHÔNG tạo mới
            }
        }
    }
    // ===== Public API / Events =====
    public event Action EquipmentChanged;

    // ===== Runtime state =====
    public Dictionary<EquipmentSlot, EquipmentData> equippedItems = new Dictionary<EquipmentSlot, EquipmentData>();
    private readonly Dictionary<EquipmentSlot, GameObject> spawnedObjects = new Dictionary<EquipmentSlot, GameObject>();
    private readonly Dictionary<EquipmentSlot, WeaponInstance> equippedWeaponInstances = new Dictionary<EquipmentSlot, WeaponInstance>();

    // Lưu bonus đã áp dụng vào playerStats theo từng slot để gỡ cho chính xác
    private readonly Dictionary<EquipmentSlot, WeaponStatBonus> appliedBonusesBySlot = new Dictionary<EquipmentSlot, WeaponStatBonus>();
    public Dictionary<EquipmentSlot, WeaponInstance> GetAllEquippedInstances()
    {
        return new Dictionary<EquipmentSlot, WeaponInstance>(equippedWeaponInstances);
    }

    public void RestoreEquipped(Dictionary<EquipmentSlot, WeaponInstance> loaded)
    {
        foreach (var kv in loaded)
        {
            EquipWeaponInstance(kv.Key, kv.Value);
        }
    }
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private WeaponHitbox currentWeaponHitbox;

    // ===== Refs =====
    private PlayerLevelSystem playerLevelSystem;
    private PlayerStatsRuntime playerStats;
    private AnimatorManager animatorManager;

    // ===== Cấu hình: áp dụng stat trang bị trực tiếp vào playerStats =====
    // Nếu = true: STR/AGI/VIT/ENE được cộng vào playerStats, và GetEquippedWeaponBonus KHÔNG trả bonus stat nữa (tránh double với UI).
    // Nếu = false: KHÔNG đụng playerStats, UI sẽ cộng bonus stat từ GetEquippedWeaponBonus như cũ.
    private const bool APPLY_EQUIP_STATS_TO_RUNTIME = true;

    [Header("Attach Points")]
    [SerializeField] private Transform weaponHoldPointR;
    [SerializeField] private Transform weaponHoldPointL;
    [SerializeField] private Transform headPoint;
    [SerializeField] private Transform bodyPoint;
    [SerializeField] private Transform handsPoint;
    [SerializeField] private Transform legsPoint;
    [SerializeField] private Transform feetPoint;
    [SerializeField] private Transform ring1Point;
    [SerializeField] private Transform ring2Point;
    [SerializeField] private Transform backPoint;

    public struct WeaponStatBonus
    {
        public int str, agi, vit, ene;
        public int mainWeaponDamage;
    }

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerLevelSystem = GetComponent<PlayerLevelSystem>();
        playerStats = playerLevelSystem != null ? playerLevelSystem.playerStats : null;
    }

    private void Start()
    {
        if (playerLevelSystem == null)
            playerLevelSystem = GetComponent<PlayerLevelSystem>();

        if (playerLevelSystem != null)
            playerStats = playerLevelSystem.playerStats;
        foreach (var kv in equippedWeaponInstances)
        {
            var inst = kv.Value;
            if (inst != null)
                ApplyBonusToRuntime(kv.Key, inst, true);
        }
        UpdateWeaponState();
    }

    public WeaponHitbox GetCurrentWeaponHitbox() => currentWeaponHitbox;

    private bool MeetsLevelRequirement(int reqLv)
    {
        if (playerLevelSystem == null) playerLevelSystem = FindAnyObjectByType<PlayerLevelSystem>();
        int playerLv = playerLevelSystem != null ? playerLevelSystem.characterData.level : 1;
        return playerLv >= reqLv;
    }

    // ====== ÁP DỤNG / GỠ BONUS VŨ KHÍ VÀO playerStats (theo slot) ======
    private static WeaponStatBonus ToBonus(WeaponInstance inst)
    {
        return new WeaponStatBonus
        {
            str = inst != null ? inst.strength : 0,
            agi = inst != null ? inst.agility : 0,
            vit = inst != null ? inst.vitality : 0,
            ene = inst != null ? inst.energy : 0,
            mainWeaponDamage = inst != null ? inst.damage : 0
        };
    }

    private void ApplyBonusToRuntime(EquipmentSlot slot, WeaponInstance inst, bool apply)
    {
        if (!APPLY_EQUIP_STATS_TO_RUNTIME || playerStats == null)
            return;

        if (apply)
        {
            // Nếu slot đã có bonus cũ (trường hợp Equip đè), gỡ trước cho chắc
            if (appliedBonusesBySlot.TryGetValue(slot, out var oldB))
            {
                playerStats.strength -= oldB.str;
                playerStats.agility -= oldB.agi;
                playerStats.vitality -= oldB.vit;
                playerStats.energy -= oldB.ene;
            }

            var b = ToBonus(inst);
            playerStats.strength += b.str;
            playerStats.agility += b.agi;
            playerStats.vitality += b.vit;
            playerStats.energy += b.ene;

            appliedBonusesBySlot[slot] = b;
        }
        else
        {
            if (appliedBonusesBySlot.TryGetValue(slot, out var b))
            {
                playerStats.strength -= b.str;
                playerStats.agility -= b.agi;
                playerStats.vitality -= b.vit;
                playerStats.energy -= b.ene;

                appliedBonusesBySlot.Remove(slot);
            }
        }

        // Tính lại các chỉ số phụ thuộc (HP/MP theo VIT/ENE)
        playerLevelSystem?.RecalculateDerivedStats();
        // Cập nhật UI
        FindAnyObjectByType<PlayerUI>()?.UpdateUI();
    }

    public void Equip(EquipmentData newItem)
    {
        if (newItem == null) return;

        if (newItem.weaponTypeID == 9)
            newItem.slot = EquipmentSlot.LeftHand;

        // Xử lý 2 tay
        if (newItem.isTwoHanded)
        {
            Unequip(EquipmentSlot.RightHand);
            Unequip(EquipmentSlot.LeftHand);
        }
        else if (newItem.slot == EquipmentSlot.RightHand &&
                 equippedItems.TryGetValue(EquipmentSlot.RightHand, out EquipmentData right) &&
                 right != null && right.isTwoHanded)
        {
            Unequip(EquipmentSlot.RightHand);
            Unequip(EquipmentSlot.LeftHand);
        }

        // Nếu slot đã có đồ -> tháo
        if (equippedItems.ContainsKey(newItem.slot))
            Unequip(newItem.slot);

        // Lưu template
        equippedItems[newItem.slot] = newItem;

        // Spawn prefab
        if (newItem.equipPrefab != null)
        {
            Transform parent = GetParentTransformForSlot(newItem.slot);
            if (parent != null)
            {
                GameObject spawned = Instantiate(newItem.equipPrefab, parent);
                spawnedObjects[newItem.slot] = spawned;
            }
        }

        // Không có stat ở EquipmentData thường (nếu có thì bạn có thể bổ sung ApplyBonusToRuntime tương tự WeaponInstance)
        EquipmentChanged?.Invoke();
        UpdateWeaponState();
        AudioManager.Instance?.PlayEquipSFX(0.9f);
    }

    public void EquipWeaponInstance(EquipmentSlot slot, WeaponInstance inst)
    {
        if (inst == null) return;

        // Gỡ slot trước (sẽ tự gỡ bonus cũ đúng 1 lần)
        Unequip(slot);

        // Gán vào runtime map
        equippedItems[slot] = inst.template;
        equippedWeaponInstances[slot] = inst;

        // Spawn prefab giữ nguyên như cũ
        if (inst.template.equipPrefab != null)
        {
            Transform parent = GetParentTransformForSlot(slot);

            // Túi cung (ID=8) gắn lưng
            if (inst.template.weaponTypeID == 8 && backPoint != null)
                parent = backPoint;

            if (parent != null)
            {
                GameObject spawned = Instantiate(inst.template.equipPrefab, parent);
                spawnedObjects[slot] = spawned;

                if (inst.template.weaponTypeID == 8)
                {
                    spawned.transform.localPosition = new Vector3(0f, 0f, 0f);
                    spawned.transform.localRotation = Quaternion.Euler(100f, 0f, 0f);
                    spawned.transform.localScale = Vector3.one;
                }
            }
            AudioManager.Instance?.PlayEquipSFX(0.9f);
        }

        // Áp bonus vào playerStats (nếu cấu hình APPLY_EQUIP_STATS_TO_RUNTIME = true)
        ApplyBonusToRuntime(slot, inst, true);

        // Gán skill cho vũ khí chính nếu có
        if (slot == EquipmentSlot.RightHand && inst.template.weaponTypeID != 8) // 8 = quiver, bỏ qua
        {
            var sm = FindAnyObjectByType<SkillManager>();
            if (sm != null)
                sm.EquipWeapon(inst);
        }

        EquipmentChanged?.Invoke();
        UpdateWeaponState();
    }

    public void UnequipAll()
    {
        var slots = new List<EquipmentSlot>(equippedItems.Keys);
        foreach (var s in slots)
            Unequip(s);

        EquipmentChanged?.Invoke();
        UpdateWeaponState();
    }

    public void Unequip(EquipmentSlot slot)
    {
        // Nếu slot có vũ khí (WeaponInstance) -> gỡ bonus trước
        if (equippedWeaponInstances.TryGetValue(slot, out var inst) && inst != null)
        {
            // Trả bonus đã áp
            ApplyBonusToRuntime(slot, inst, false);
        }

        bool hadSpawn = spawnedObjects.TryGetValue(slot, out var obj) && obj != null;
        bool hadTemplate = equippedItems.ContainsKey(slot);
        bool hadInst = equippedWeaponInstances.ContainsKey(slot);

        if (hadSpawn) Destroy(obj);
        spawnedObjects.Remove(slot);

        bool removedTemplate = equippedItems.Remove(slot);
        bool removedInst = equippedWeaponInstances.Remove(slot);

        if (hadSpawn || hadTemplate || hadInst || removedTemplate || removedInst)
            AudioManager.Instance?.PlayUnequipSFX(0.9f);
    }

    public int CurrentWeaponType()
    {
        if (equippedItems.TryGetValue(EquipmentSlot.LeftHand, out var left) && left != null)
        {
            if (left.weaponTypeID == 3) // Bow
                return 3;
        }

        if (equippedItems.TryGetValue(EquipmentSlot.RightHand, out var right) && right != null)
        {
            if (right.weaponTypeID != 8) // 8 = Quiver
                return right.weaponTypeID;
        }

        if (equippedItems.TryGetValue(EquipmentSlot.LeftHand, out var l) && l != null)
            return l.weaponTypeID;

        return 0;
    }

    public WeaponStatBonus GetEquippedWeaponBonus()
    {
        WeaponStatBonus b = default;

        foreach (var kv in equippedWeaponInstances)
        {
            var inst = kv.Value;
            if (inst == null) continue;

            // Nếu đã APPLY vào runtime, KHÔNG cộng lại STR/AGI/VIT/ENE để tránh double với UI (UI đang làm stats + bonus)
            if (!APPLY_EQUIP_STATS_TO_RUNTIME)
            {
                b.str += inst.strength;
                b.agi += inst.agility;
                b.vit += inst.vitality;
                b.ene += inst.energy;
            }
            // Damage của vũ khí vẫn cộng qua bonus (để DamageCalculator dùng)
            b.mainWeaponDamage += inst.damage;
        }

        return b;
    }

    private void UpdateWeaponState()
    {
        if (playerStats == null) return;

        int finalWeaponTypeID = 0;
        currentWeaponHitbox = null;

        if (spawnedObjects.TryGetValue(EquipmentSlot.RightHand, out GameObject rightObj) && rightObj != null)
        {
            finalWeaponTypeID = equippedItems[EquipmentSlot.RightHand].weaponTypeID;
            currentWeaponHitbox = rightObj.GetComponentInChildren<WeaponHitbox>();
        }
        else if (spawnedObjects.TryGetValue(EquipmentSlot.LeftHand, out GameObject leftObj) && leftObj != null)
        {
            finalWeaponTypeID = equippedItems[EquipmentSlot.LeftHand].weaponTypeID;
            currentWeaponHitbox = leftObj.GetComponentInChildren<WeaponHitbox>();
        }

        animatorManager.SetFloat(animatorManager.WeaponTypeHash, finalWeaponTypeID);
        animatorManager.SetBool(animatorManager.IsEquippedHash, equippedItems.Count > 0);

        if (currentWeaponHitbox != null)
        {
            var bonus = GetEquippedWeaponBonus();
            int totalDamage = DamageCalculator.GetFinalDamage(playerStats, bonus);
            currentWeaponHitbox.SetDamage(totalDamage);
        }
    }

    public bool HasItemInSlot(EquipmentSlot slot) => equippedItems.ContainsKey(slot);

    public bool IsEquipped(WeaponInstance inst, out EquipmentSlot eqSlot)
    {
        foreach (var kvp in equippedWeaponInstances)
        {
            if (kvp.Value == inst)
            {
                eqSlot = kvp.Key;
                return true;
            }
        }
        eqSlot = default;
        return false;
    }

    public bool TryEquipIfSlotFree(EquipmentSlot slot, WeaponInstance inst)
    {
        if (!equippedItems.ContainsKey(slot))
        {
            EquipWeaponInstance(slot, inst);
            Inventory.Instance?.RemoveWeapon(inst);
            return true;
        }
        return false;
    }

    public bool TryGetEquippedInstance(EquipmentSlot slot, out WeaponInstance inst)
    {
        if (equippedWeaponInstances.TryGetValue(slot, out inst))
            return inst != null;
        inst = null;
        return false;
    }

    public void EquipFromInventory(EquipmentSlot slot, WeaponInstance inst)
    {
        if (inst == null) return;

        if (Inventory.Instance != null)
        {
            bool removed = Inventory.Instance.RemoveWeapon(inst);
            Debug.Log($"[EquipmentManager] RemoveWeapon {inst.template.weaponName} => {removed}");
        }

        EquipWeaponInstance(slot, inst);

        if (InventoryUI.Instance != null)
            InventoryUI.Instance.RefreshUI();

        if (playerLevelSystem != null)
        {
            playerLevelSystem.RecalculateDerivedStats();
            playerLevelSystem.UpdateUI();
        }
    }

    public void UnequipSlot(EquipmentSlot slot)
    {
        if (equippedWeaponInstances.TryGetValue(slot, out var inst) && inst != null)
            Inventory.Instance?.AddWeapon(inst);

        Unequip(slot);
        EquipmentChanged?.Invoke();
    }

    private Transform GetParentTransformForSlot(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.RightHand: return weaponHoldPointR;
            case EquipmentSlot.LeftHand: return weaponHoldPointL;
            case EquipmentSlot.Head: return headPoint;
            case EquipmentSlot.Body: return bodyPoint;
            case EquipmentSlot.Hands: return handsPoint;
            case EquipmentSlot.Legs: return legsPoint;
            case EquipmentSlot.Feet: return feetPoint;
            case EquipmentSlot.Ring1: return ring1Point;
            case EquipmentSlot.Ring2: return ring2Point;
        }
        return null;
    }

    // Animation events forwarders
    public void EnableCurrentWeaponHitbox() { if (currentWeaponHitbox) currentWeaponHitbox.EnableHitbox(); }
    public void DisableCurrentWeaponHitbox() { if (currentWeaponHitbox) currentWeaponHitbox.DisableHitbox(); }

    public Transform GetWeaponHoldPointL() => weaponHoldPointL;
    public Transform GetWeaponHoldPointR() => weaponHoldPointR;  
}
