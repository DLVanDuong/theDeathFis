using UnityEngine;

public class PlayerDebugCheat : MonoBehaviour
{
    private PlayerLevelSystem levelSystem;
    private PlayerStatsRuntime stats;
    private HealthPlayer healthUI;
    private PlayerUI playerUI;

    [Header("Cheat Drop")]
    [SerializeField] private WeaponData plus5Template;   // kéo 1 template vào đây
    [SerializeField] private WeaponData plus10Template;  // kéo 1 template vào đây
    [SerializeField] private GameObject weaponDropPrefab;
    [SerializeField] private WeaponRarity plus5Rarity = WeaponRarity.Epic;
    [SerializeField] private WeaponRarity plus10Rarity = WeaponRarity.Legendary;
    [SerializeField] private float dropForward = 1.6f;
    [SerializeField] private float dropSpread = 0.7f;

    void Awake()
    {
        levelSystem = GetComponent<PlayerLevelSystem>();
        if (levelSystem != null)
            stats = levelSystem.playerStats;

        healthUI = GetComponent<HealthPlayer>();
        playerUI = FindAnyObjectByType<PlayerUI>();
    }

    void Update()
    {
        // === F1: Buff cực mạnh ===
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var level = FindAnyObjectByType<PlayerLevelSystem>();
            if (level != null && level.playerStats != null)
            {
                var s = level.playerStats;
                s.level = 200;
                s.cheatBonusAttack = 10000;
                s.cheatBonusHP = 100000;
                s.cheatBonusMP = 1000;
                s.isCheatBuffActive = true; // ✅ Đánh dấu buff đang bật
                level.RecalculateDerivedStats(true);
                Debug.Log("<color=yellow>[CHEAT] F1 Buff: +10000 ATK, +100000 HP, +1000 MP!</color>");
            }
        }
        // === F2: Hồi đầy máu và mana ===
        if (Input.GetKeyDown(KeyCode.F2))
        {
            var level = FindAnyObjectByType<PlayerLevelSystem>();
            if (level != null && level.playerStats != null)
            {
                var s = level.playerStats;              
                s.statPoints = 20000;
                s.level = 200;
                s.currentHealth = s.maxHealth;
                s.currentMana = s.maxMana;

                var hpUI = FindAnyObjectByType<HealthPlayer>();
                var playerUI = FindAnyObjectByType<PlayerUI>();
                hpUI?.UpdateMaxStats();
                playerUI?.UpdateUI();

                Debug.Log("<color=green>[CHEAT] F2: Hồi đầy máu và mana!</color>");
            }
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            var level = FindAnyObjectByType<PlayerLevelSystem>();
            if (level != null && level.playerStats != null)
            {
                var s = level.playerStats;
                s.cheatBonusAttack = 0;
                s.cheatBonusHP = 0;
                s.cheatBonusMP = 0;
                s.isCheatBuffActive = false; // ✅ Hủy cờ buff
                level.RecalculateDerivedStats(true);
                Debug.Log("<color=red>[CHEAT] F3: Reset buff về bình thường!</color>");
            }
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            var level = FindAnyObjectByType<PlayerLevelSystem>();
            if (level != null && level.playerStats != null)
            {
                var s = level.playerStats;              
                s.currentHealth = s.maxHealth;
                s.currentMana = s.maxMana;

                var hpUI = FindAnyObjectByType<HealthPlayer>();
                var playerUI = FindAnyObjectByType<PlayerUI>();
                hpUI?.UpdateMaxStats();
                playerUI?.UpdateUI();

                Debug.Log("<color=green>[CHEAT] F2: Hồi đầy máu và mana!</color>");
            }
        }
    }
   
}

