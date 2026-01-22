using UnityEngine;
using System;

[Serializable]
public class WeaponInstance
{
    public string instanceId;
    public WeaponData template;

    public WeaponRarity rarity = WeaponRarity.Common;
    public int upgradeLevel = 0;

    public int damage;
    public int strength, agility, vitality, energy;
    public int requiredLevel;

    public SkillData skill1, skill2;

    public WeaponInstance(WeaponData data, WeaponRarity rarity = WeaponRarity.Common, int upgrade = 0)
    {
        template = data;
        this.rarity = rarity;
        upgradeLevel = Mathf.Clamp(upgrade, 0, 10);

        instanceId = Guid.NewGuid().ToString();

        // base from template
        damage = data.baseDamage;
        strength = data.baseStrength;
        agility = data.baseAgility;
        vitality = data.baseVitality;
        energy = data.baseEnergy;
        requiredLevel = data.requiredLevel;

        skill1 = data.skill1;
        skill2 = data.skill2;

        ApplyUpgradeBonus();
    }

    public void ApplyUpgradeBonus()
    {
        // gộp 1 lần, KHÔNG set lại rarity
        float reqScale = 1f + 0.5f * Mathf.Max(0, requiredLevel - 1);
        float upgScale = 1f + 0.5f * Mathf.Max(0, upgradeLevel);
        float mul = reqScale * upgScale;

        damage = Mathf.RoundToInt(template.baseDamage * mul);
        strength = Mathf.RoundToInt(template.baseStrength * mul);
        agility = Mathf.RoundToInt(template.baseAgility * mul);
        vitality = Mathf.RoundToInt(template.baseVitality * mul);
        energy = Mathf.RoundToInt(template.baseEnergy * mul);
    }
    public bool CanUpgrade(int maxLevel = 10)
    {
        return upgradeLevel < maxLevel;
    }

    public void UpgradeOnce(int maxLevel = 10)
    {
        if (!CanUpgrade(maxLevel)) return;

        upgradeLevel++;
        ApplyUpgradeBonus();
    }
    [Serializable]
    public class Save
    {
        public string instanceId;
        public string templateKey;
        public int rarity;
        public int upgradeLevel;

        public int damage, strength, agility, vitality, energy;
        public int requiredLevel;
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
            requiredLevel = requiredLevel
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

        // ❗ tạo instance bằng rarity/+level ĐÃ LƯU
        var inst = new WeaponInstance(tpl, (WeaponRarity)s.rarity, s.upgradeLevel);
        inst.instanceId = string.IsNullOrEmpty(s.instanceId) ? Guid.NewGuid().ToString() : s.instanceId;

        // nếu bạn lưu stat đã roll → ghi đè lại
        inst.damage = s.damage;
        inst.strength = s.strength;
        inst.agility = s.agility;
        inst.vitality = s.vitality;
        inst.energy = s.energy;
        inst.requiredLevel = s.requiredLevel;

        return inst;
    }
}
