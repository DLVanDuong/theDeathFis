using UnityEngine;

[System.Serializable]
public class PlayerStatsRuntime
{
    public int level;
    public float exp;
    public float expToNextLevel;

    public float maxHealth;
    public float currentHealth;

    [Header("Tấn công")]
    public int baseAttack; // Attack gốc từ ScriptableObject
    public int statPoints;

    [Header("Modifier")]
    public float attackModifier = 1f;

    [Header("Chỉ số nhân vật")]
    public float strength;
    public float agility;
    public float vitality;
    public float energy;

    public float maxMana;
    public float currentMana;

    // === CHEAT BONUS ===
    public int cheatBonusAttack = 0;
    public int cheatBonusHP = 0;
    public int cheatBonusMP = 0;

    [HideInInspector] public bool isCheatBuffActive = false;

    // Attack sẽ được tính dựa trên BaseAttack + chỉ số
    public int Attack
    {
        get
        {
            int raw = baseAttack + Mathf.RoundToInt(strength * 2f + agility * 1f);
            return Mathf.RoundToInt(raw * attackModifier);
        }
    }
   
    public PlayerStatsRuntime(Character initialStats)
    {
        this.level = initialStats.level;
        this.exp = initialStats.currentExp;
        this.expToNextLevel = initialStats.expToNextLevel;

        this.maxHealth = initialStats.maxHealth;
        this.currentHealth = initialStats.maxHealth;

        this.baseAttack = initialStats.attack;  // Attack gốc
        this.statPoints = initialStats.statPoints;

        this.strength = initialStats.strength;
        this.agility = initialStats.agility;
        this.vitality = initialStats.vitality;
        this.energy = initialStats.energy;

        this.maxMana = initialStats.maxMana;
        this.currentMana = initialStats.maxMana;
    }
}
