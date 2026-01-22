using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthPlayer : MonoBehaviour
{
    [Header("Refs")]
    public PlayerLevelSystem levelSystem;

    [Header("UI")]
    public Slider hpSlider;
    public Slider mpSlider;
    public Image hpFill;
    public Image mpFill;
    public TextMeshProUGUI hpSmallText;
    public TextMeshProUGUI mpSmallText;

    [Header("Death & Respawn (Same Map)")]
    public GameObject deathUIPanel;      // gán Panel UI chết
    public float respawnDelay = 3f;      // chờ 3 giây
    public string respawnTag = "TownSpawn"; // tag của điểm hồi sinh

    private bool isDead = false;     // chờ vài giây trước khi về thành
  
    void Awake()
    {
        if (!levelSystem) levelSystem = GetComponent<PlayerLevelSystem>();
    }

    void Start()
    {
        if (levelSystem == null || levelSystem.playerStats == null) return;
        levelSystem.RecalculateDerivedStats();
        SyncFromStats(true);
    }
    private void Update()
    {
        // Khi HP <= 0 thì kích hoạt chết (script gốc của bạn chưa có phần này) :contentReference[oaicite:1]{index=1}
        if (!isDead && levelSystem != null && levelSystem.playerStats != null)
        {
            if (levelSystem.playerStats.currentHealth <= 0)
                HandleDeath();
        }
    }
    // Gọi khi Max HP/MP thay đổi
    public void UpdateMaxStats() => SyncFromStats(false);

    public void RestoreHealth(int amount)
    {
        var s = levelSystem.playerStats;
        s.currentHealth = Mathf.Min(s.currentHealth + amount, s.maxHealth);
        Debug.Log($"[HealthPlayer] RestoreHealth {amount} => {s.currentHealth}/{s.maxHealth}");
        SyncFromStats(false);
    }

    public void RestoreMana(int amount)
    {
        var s = levelSystem.playerStats;
        s.currentMana = Mathf.Min(s.currentMana + amount, s.maxMana);
        Debug.Log($"[HealthPlayer] RestoreMana {amount} => {s.currentMana}/{s.maxMana}");
        SyncFromStats(false);
    }

    public void TakeDamage(int dmg)
    {
        AudioManager.Instance?.PlaySFXShort(AudioManager.Instance.playerHit, 0.4f, 1f);

        if (Resources.Load<GameObject>("BloodFX") is GameObject bloodFX)
        {
            GameObject fx = GameObject.Instantiate(bloodFX, transform.position + Vector3.up * 1.2f, Quaternion.identity);
            GameObject.Destroy(fx, 1f);
        }
        var s = levelSystem.playerStats;
        s.currentHealth = Mathf.Max(0, s.currentHealth - dmg);
        Debug.Log($"[HealthPlayer] TakeDamage {dmg} => {s.currentHealth}/{s.maxHealth}");
        SyncFromStats(false);
    }
    
    private void SyncFromStats(bool resetCurrents)
    {
        var s = levelSystem.playerStats;
        if (resetCurrents)
        {
            s.currentHealth = s.maxHealth;
            s.currentMana = s.maxMana;
        }

        if (hpSlider)
        {
            hpSlider.maxValue = s.maxHealth;
            hpSlider.value = s.currentHealth;
        }
        if (mpSlider)
        {
            mpSlider.maxValue = s.maxMana;
            mpSlider.value = s.currentMana;
        }

        if (hpFill) hpFill.fillAmount = s.maxHealth <= 0 ? 0 : s.currentHealth / s.maxHealth;
        if (mpFill) mpFill.fillAmount = s.maxMana <= 0 ? 0 : s.currentMana / s.maxMana;

        if (hpSmallText) hpSmallText.text = $"{Mathf.CeilToInt(s.currentHealth)}";
        if (mpSmallText) mpSmallText.text = $"{Mathf.CeilToInt(s.currentMana)}";

        FindAnyObjectByType<PlayerUI>()?.UpdateUI();
    }

    public float GetCurrentMana() => levelSystem.playerStats.currentMana;

    private void HandleDeath()
    {
        isDead = true;

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.playerDeath, 1f);
       
        var controller = GetComponent<PlayerStateMachine>();
        if (controller) controller.enabled = false;

        
        if (deathUIPanel) deathUIPanel.SetActive(true);

        var psm = GetComponent<PlayerStateMachine>();
        if (psm) psm.OnPlayerDeath_HardStop();  
        

        Invoke(nameof(RespawnPlayer), respawnDelay);
    }
    private void RespawnPlayer()
    {
        // Tìm vị trí hồi sinh
        Vector3 spawnPos = transform.position;
        var point = GameObject.FindGameObjectWithTag(respawnTag);
        if (point != null) spawnPos = point.transform.position;

        // Nếu có CharacterController thì disable tạm để không bị chặn khi teleport
        var cc = GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            transform.position = spawnPos;
            cc.enabled = true;
        }
        else
        {
            transform.position = spawnPos;
        }

        // Hồi máu/mana đầy
        var s = levelSystem.playerStats;
        s.currentHealth = s.maxHealth;
        s.currentMana = s.maxMana;
        SyncFromStats(true);

        // Ẩn UI chết
        if (deathUIPanel) deathUIPanel.SetActive(false);

        // Bật lại điều khiển
        var controller = GetComponent<PlayerStateMachine>();
        if (controller) controller.enabled = true;

        isDead = false;
    }
}
