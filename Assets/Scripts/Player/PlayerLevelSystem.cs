using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
    public PlayerStatsRuntime playerStats;
    [SerializeField] public Character characterData;
    private HealthPlayer playerHealth;
    public GameObject levelUpEffectPrefab;

    [Header("Skill System")]
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private List<SkillUnlock> skillsToUnlock;

    [System.Serializable]
    public class SkillUnlock { public SkillData skill; public int level; }

    private const int HP_PER_VIT = 10;
    private const int MP_PER_ENE = 10;
    private const float CD_PER_AGI = 0.001f;
    private const float MIN_CD_SCALE = 0.40f;

    private int baselineVIT, baselineENE, baselineAGI;

    void Awake()
    {
        if (characterData != null)
        {
            playerStats = new PlayerStatsRuntime(characterData);
        }

        baselineVIT = (int)(characterData ? characterData.vitality : playerStats.vitality);
        baselineENE = (int)(characterData ? characterData.energy : playerStats.energy);
        baselineAGI = (int)(characterData ? characterData.agility : playerStats.agility);

        skillManager = GetComponent<SkillManager>();
        playerHealth = GetComponent<HealthPlayer>();

        RecalculateDerivedStats();
        UpdateUI();
    }

    public void AddStatPoint(string statName, int amount)
    {
        if (playerStats == null) return;
        if (playerStats.statPoints <= 0) return;

        // 🚫 Không cho cộng điểm khi đang bật buff
        if (playerStats.isCheatBuffActive || playerStats.cheatBonusAttack > 0 || playerStats.cheatBonusHP > 0)
        {
            return;
        }
               
        amount = Mathf.Min(amount, playerStats.statPoints);
        // ✅ Cộng điểm bình thường
        switch (statName)
        {
            case "Strength": playerStats.strength += amount; break;
            case "Agility": playerStats.agility += amount; break;
            case "Vitality": playerStats.vitality += amount; break;
            case "Energy": playerStats.energy += amount; break;
        }

        playerStats.statPoints -= amount;

        // ✅ Tính lại chỉ số (giữ nguyên HP/MP, không reset buff vì đã bị chặn trên)
        RecalculateDerivedStats();
        UpdateUI();
    }

    public void RecalculateDerivedStats(bool keepCheat = true)
    {
        int vitDelta = (int)Mathf.Max(0, playerStats.vitality - baselineVIT);
        float baseHP = characterData ? characterData.maxHealth : 100f;
        playerStats.maxHealth = baseHP + vitDelta * HP_PER_VIT;

        int eneDelta = (int)Mathf.Max(0, playerStats.energy - baselineENE);
        float baseMP = characterData ? characterData.maxMana : 50f;
        playerStats.maxMana = baseMP + eneDelta * MP_PER_ENE;

        // ✅ Nếu đang buff, vẫn giữ buff cộng thêm
        if (keepCheat && playerStats.isCheatBuffActive)
        {
            playerStats.maxHealth += playerStats.cheatBonusHP;
            playerStats.maxMana += playerStats.cheatBonusMP;
            playerStats.baseAttack += playerStats.cheatBonusAttack;
        }

        // Giữ HP/MP hợp lệ
        playerStats.currentHealth = Mathf.Min(playerStats.currentHealth, playerStats.maxHealth);
        playerStats.currentMana = Mathf.Min(playerStats.currentMana, playerStats.maxMana);

        // Cập nhật UI
        playerHealth?.UpdateMaxStats();
        FindAnyObjectByType<PlayerUI>()?.UpdateUI();
    }

    private void ApplyCheatBuffs()
    {
        if (playerStats == null) return;

        // ⚡ Cộng buff
        playerStats.maxHealth += playerStats.cheatBonusHP;
        playerStats.maxMana += playerStats.cheatBonusMP;
        playerStats.baseAttack += playerStats.cheatBonusAttack;

        // Đảm bảo HP/MP không vượt max
        playerStats.currentHealth = Mathf.Min(playerStats.currentHealth, playerStats.maxHealth);
        playerStats.currentMana = Mathf.Min(playerStats.currentMana, playerStats.maxMana);
    }
    public void AddExperience(int exp)
    {
        playerStats.exp += exp;

        // Loop để xử lý lên nhiều cấp nếu EXP dư
        while (playerStats.exp >= playerStats.expToNextLevel)
        {
            playerStats.exp -= playerStats.expToNextLevel;
            playerStats.level++;
            playerStats.statPoints += 10;

            ShowLevelUpEffect();
            UnlockSkills();
            UpdateStats();

            // Cập nhật expToNextLevel theo level mới
            playerStats.expToNextLevel = playerStats.level * 100;
        }

        RecalculateDerivedStats(true);
        UpdateUI();
    }
    private void ShowLevelUpEffect()
    {
        if (levelUpEffectPrefab != null)
        {
            Vector3 spawnPoint = transform.position;
            GameObject effect = Instantiate(levelUpEffectPrefab, spawnPoint, Quaternion.identity);
            Destroy(effect, 1.0f);
        }
    }

    // Mở skill khi đạt level
    private void UnlockSkills()
    {
        if (skillManager == null || skillsToUnlock == null) return;

        foreach (var skillUnlock in skillsToUnlock)
        {
            if (playerStats.level >= skillUnlock.level)
            {
                skillManager.UnlockSkill(skillUnlock.skill);
            }
        }
    }

    // Cập nhật chỉ số sau khi lên level
    private void UpdateStats()
    {
        // Gọi lại tính toán HP/MP/ATK từ stat
        RecalculateDerivedStats();
    }
    public void UpdateUI()
    {
        var ui = FindAnyObjectByType<PlayerUI>();
        if (ui != null) ui.UpdateUI();
    }
}