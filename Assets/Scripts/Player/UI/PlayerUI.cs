using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerLevelSystem levelSystem;

    [Header("UI Text")]

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelTexts;
    public TextMeshProUGUI hpCharacter;
    public TextMeshProUGUI mpCharacter;
    public TextMeshProUGUI atkCharacter;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI statPointsText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI agilityText;
    public TextMeshProUGUI vitalityText;
    public TextMeshProUGUI energyText;
    private int addAmount = 1;
    [Header("Add Buttons")]
    public Button strAddButton;
    public Button agiAddButton;
    public Button vitAddButton;
    public Button eneAddButton;

    private void Awake()
    {
        if (levelSystem == null)
            levelSystem = FindAnyObjectByType<PlayerLevelSystem>();

        var equipMgr = FindAnyObjectByType<EquipmentManager>();
        if (equipMgr != null)
        {
            equipMgr.EquipmentChanged += UpdateUI;
        }

        // Gán sự kiện cho các nút cộng điểm
        if (strAddButton) strAddButton.onClick.AddListener(() => AddStat("Strength"));
        if (agiAddButton) agiAddButton.onClick.AddListener(() => AddStat("Agility"));
        if (vitAddButton) vitAddButton.onClick.AddListener(() => AddStat("Vitality"));
        if (eneAddButton) eneAddButton.onClick.AddListener(() => AddStat("Energy"));
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    private void AddStat(string statName)
    {
        if (levelSystem != null)
        {
            levelSystem.AddStatPoint(statName, addAmount);
            UpdateUI();
        }
    }
  
    public void UpdateUI()
    {
        if (levelSystem == null || levelSystem.playerStats == null) return;

        var stats = levelSystem.playerStats;

        // Lấy bonus từ vũ khí
        var equipMgr = FindAnyObjectByType<EquipmentManager>();
        var bonus = equipMgr != null ? equipMgr.GetEquippedWeaponBonus() : default;

        int finalAtk = DamageCalculator.GetFinalDamage(stats, bonus);

        if (atkCharacter) atkCharacter.text = $"ATK: {finalAtk}";

        int finalStr = (int)stats.strength + bonus.str;
        int finalAgi = (int)stats.agility + bonus.agi;
        int finalVit = (int)stats.vitality + bonus.vit;
        int finalEne = (int)stats.energy + bonus.ene;
             
        // Hiển thị UI
        if (levelText) levelText.text = $"Level: {stats.level}";
        if (levelTexts) levelTexts.text = $"{stats.level}";
        if (expText) expText.text = $"EXP: {stats.exp:F0} / {stats.expToNextLevel:F0}";
        if (statPointsText) statPointsText.text = $"Điểm cộng: {stats.statPoints}";
        if (atkCharacter) atkCharacter.text = $"ATK: {finalAtk}";
        if (strengthText) strengthText.text = $"Sức mạnh: {finalStr}";
        if (agilityText) agilityText.text = $"Nhanh nhẹn: {finalAgi}";
        if (vitalityText) vitalityText.text = $"Thể lực: {finalVit}";
        if (energyText) energyText.text = $"Năng lượng: {finalEne}";

        if (hpCharacter) hpCharacter.text = $"HP: {Mathf.CeilToInt(stats.currentHealth)} / {Mathf.CeilToInt(stats.maxHealth)}";
        if (mpCharacter) mpCharacter.text = $"MP: {Mathf.CeilToInt(stats.currentMana)} / {Mathf.CeilToInt(stats.maxMana)}";
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            addAmount = 5;
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            addAmount = 10;
        else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            addAmount = levelSystem != null ? levelSystem.playerStats.statPoints : 1;
        else
            addAmount = 1;
    }
    private void OnDestroy()
    {
        var equipMgr = FindAnyObjectByType<EquipmentManager>();
        if (equipMgr != null)
            equipMgr.EquipmentChanged -= UpdateUI;
    }
}
