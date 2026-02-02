using UnityEngine;
using System;

[Serializable]
public class WeaponInstance
{
    public string instanceId;
    public WeaponData template;

    public WeaponRarity rarity = WeaponRarity.Common;
    public int upgradeLevel = 0;

    // ===== STAT HIỆN TẠI (đang dùng để hiển thị/đánh) =====
    public int damage;
    public int strength, agility, vitality, energy;
    public int requiredLevel;

    public SkillData skill1, skill2;

    public bool hasElementStone = false;
    public UpgradeStoneType elementStone = UpgradeStoneType.Stone_Fire; // hoặc Stone_Wind, tuỳ bạn

    public float elementCraftBonus = 0f;

    // ===== BASE ĐÃ ROLL (CHỐT LẠI sau khi drop/roll/scale enemy level) =====
    [SerializeField] private int baseDamageRolled;
    [SerializeField] private int baseStrRolled;
    [SerializeField] private int baseAgiRolled;
    [SerializeField] private int baseVitRolled;
    [SerializeField] private int baseEneRolled;

    private const float DMG_UPGRADE_PER_LEVEL = 0.05f;      // +1 = +5% DMG
    private const float SUBSTAT_UPGRADE_PER_LEVEL = 0.02f;  // +1 = +2% STR/AGI/VIT/ENE

    public WeaponInstance(WeaponData data, WeaponRarity rarity = WeaponRarity.Common, int upgrade = 0)
    {
        template = data;
        this.rarity = rarity;
        upgradeLevel = Mathf.Clamp(upgrade, 0, 10);

        instanceId = Guid.NewGuid().ToString();

        // base từ template (lúc mới tạo)
        damage = data.baseDamage;
        strength = data.baseStrength;
        agility = data.baseAgility;
        vitality = data.baseVitality;
        energy = data.baseEnergy;
        requiredLevel = data.requiredLevel;

        skill1 = data.skill1;
        skill2 = data.skill2;

        // Chốt base ban đầu (sẽ được overwrite lại sau khi roll/drop)
        CaptureRolledBase();

        // Apply upgrade theo baseRolled
        ApplyUpgradeBonus();
    }

    /// <summary>
    /// Gọi sau khi bạn đã roll stat / cộng theo level enemy / chỉnh dmg…
    /// để chốt "base đã roll" làm nền cho upgrade.
    /// </summary>
    public void CaptureRolledBase()
    {
        baseDamageRolled = damage;
        baseStrRolled = strength;
        baseAgiRolled = agility;
        baseVitRolled = vitality;
        baseEneRolled = energy;
    }

    public void ApplyUpgradeBonus()
    {
        // Nếu baseRolled chưa có (trường hợp item cũ), fallback về stat hiện tại
        if (baseDamageRolled <= 0) baseDamageRolled = Mathf.Max(1, damage);
        if (baseStrRolled <= 0) baseStrRolled = Mathf.Max(0, strength);
        if (baseAgiRolled <= 0) baseAgiRolled = Mathf.Max(0, agility);
        if (baseVitRolled <= 0) baseVitRolled = Mathf.Max(0, vitality);
        if (baseEneRolled <= 0) baseEneRolled = Mathf.Max(0, energy);

        float dmgMul = 1f + (upgradeLevel * DMG_UPGRADE_PER_LEVEL);
        float subMul = 1f + (upgradeLevel * SUBSTAT_UPGRADE_PER_LEVEL);

        damage = Mathf.RoundToInt(baseDamageRolled * dmgMul);
        strength = Mathf.RoundToInt(baseStrRolled * subMul);
        agility = Mathf.RoundToInt(baseAgiRolled * subMul);
        vitality = Mathf.RoundToInt(baseVitRolled * subMul);
        energy = Mathf.RoundToInt(baseEneRolled * subMul);
    }

    public bool CanUpgrade(int maxLevel = 10) => upgradeLevel < maxLevel;

    public void UpgradeOnce(int maxLevel = 10)
    {
        if (!CanUpgrade(maxLevel)) return;
        upgradeLevel++;
        ApplyUpgradeBonus();
    }

    // ================= SAVE / LOAD =================

    [Serializable]
    public class Save
    {
        public string instanceId;
        public string templateKey;
        public int rarity;
        public int upgradeLevel;

        public bool hasElementStone;
        public int elementStone;

        public float elementCraftBonus;

        public int damage, strength, agility, vitality, energy;
        public int requiredLevel;

        // NEW: lưu luôn baseRolled để upgrade không reset
        public int baseDamageRolled, baseStrRolled, baseAgiRolled, baseVitRolled, baseEneRolled;
    }

    public Save ToSave()
    {
        if (string.IsNullOrEmpty(instanceId))
            instanceId = Guid.NewGuid().ToString();

        return new Save
        {
            instanceId = instanceId,
            templateKey = template != null ? template.saveKey : "",
            rarity = (int)rarity,
            upgradeLevel = upgradeLevel,

            damage = damage,
            strength = strength,
            agility = agility,
            vitality = vitality,
            energy = energy,
            requiredLevel = requiredLevel,

            hasElementStone = hasElementStone,
            elementStone = (int)elementStone,

            elementCraftBonus = elementCraftBonus,

            baseDamageRolled = baseDamageRolled,
            baseStrRolled = baseStrRolled,
            baseAgiRolled = baseAgiRolled,
            baseVitRolled = baseVitRolled,
            baseEneRolled = baseEneRolled
        };
    }

    public static WeaponInstance FromSave(Save s, Func<string, WeaponData> getTemplateByKey)
    {
        var tpl = getTemplateByKey != null ? getTemplateByKey(s.templateKey) : null;
        if (tpl == null)
        {
            Debug.LogWarning($"[WeaponInstance.FromSave] Không tìm thấy template cho key='{s.templateKey}'.");
            return null;
        }

        var inst = new WeaponInstance(tpl, (WeaponRarity)s.rarity, s.upgradeLevel);
        inst.instanceId = string.IsNullOrEmpty(s.instanceId) ? Guid.NewGuid().ToString() : s.instanceId;

        inst.requiredLevel = s.requiredLevel;

        // Restore baseRolled (ưu tiên)
        inst.baseDamageRolled = s.baseDamageRolled;
        inst.baseStrRolled = s.baseStrRolled;
        inst.baseAgiRolled = s.baseAgiRolled;
        inst.baseVitRolled = s.baseVitRolled;
        inst.baseEneRolled = s.baseEneRolled;

        inst.hasElementStone = s.hasElementStone;
        inst.elementStone = (UpgradeStoneType)s.elementStone;

        inst.elementCraftBonus = s.elementCraftBonus;

        // Backward compatibility: nếu file save cũ chưa có baseRolled
        if (inst.baseDamageRolled <= 0)
        {
            float dmgMul = 1f + (inst.upgradeLevel * DMG_UPGRADE_PER_LEVEL);
            float subMul = 1f + (inst.upgradeLevel * SUBSTAT_UPGRADE_PER_LEVEL);

            inst.baseDamageRolled = Mathf.Max(1, Mathf.RoundToInt(s.damage / dmgMul));
            inst.baseStrRolled = Mathf.RoundToInt(s.strength / subMul);
            inst.baseAgiRolled = Mathf.RoundToInt(s.agility / subMul);
            inst.baseVitRolled = Mathf.RoundToInt(s.vitality / subMul);
            inst.baseEneRolled = Mathf.RoundToInt(s.energy / subMul);
        }

        // set stat hiện tại theo upgrade
        inst.ApplyUpgradeBonus();
        return inst;
    }
}
