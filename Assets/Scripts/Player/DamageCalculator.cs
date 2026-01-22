using UnityEngine;

public static class DamageCalculator
{
    public static int GetFinalDamage(PlayerStatsRuntime stats, EquipmentManager.WeaponStatBonus bonus, SkillData skill = null)
    {
        if (stats == null) return 0;

        int raw = stats.baseAttack
                + Mathf.RoundToInt(stats.strength * 2f + stats.agility * 1f);

        // Cộng thêm bonus từ vũ khí
        raw += bonus.str * 2;   // STR bonus từ vũ khí
        raw += bonus.agi * 1;   // AGI bonus từ vũ khí
        raw += bonus.mainWeaponDamage; // damage gốc của vũ khí

        if (skill != null)
            raw += Mathf.RoundToInt(skill.damage);

        return Mathf.RoundToInt(raw * stats.attackModifier);
    }
    public static int CalculateSkillDamage(PlayerStatsRuntime stats, SkillData skill, WeaponInstance weapon)
    {
        if (stats == null)
        {
            Debug.LogWarning("[DamageCalculator] PlayerStatsRuntime NULL — skill damage = 1");
            return 1;
        }

        // Lấy damage cơ bản từ stat
        float baseDmg = stats.strength * 0.5f + stats.energy * 0.3f;

        // Nếu có vũ khí
        if (weapon != null)
            baseDmg += weapon.damage;

        // Nhân với hệ số trong SkillData
        if (skill != null)
            baseDmg *= skill.aoeDamage;

        return Mathf.RoundToInt(baseDmg);
    }
    public static int GetAOEDamage(PlayerStatsRuntime playerStats, SkillData skill, WeaponData weapon = null)
    {
        if (playerStats == null || skill == null) return 0;

        int damage = playerStats.Attack;

        if (weapon != null)
            damage += weapon.baseDamage;

        damage += Mathf.RoundToInt(skill.aoeDamage);

        return damage;
    }
}