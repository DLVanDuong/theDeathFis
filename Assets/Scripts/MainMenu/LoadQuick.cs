using UnityEngine;

public class LoadQuick : MonoBehaviour
{
    [SerializeField] PlayerLevelSystem levelSys;
    [SerializeField] Transform player;                 // Kéo Transform Player vào
    [SerializeField] EquipmentManager equip;           // Kéo EquipmentManager Player vào
    [SerializeField] WeaponDatabase weaponDB;          // Kéo WeaponDatabase vào

    public void LoadNow()
    {
        if (!SaveSystem.HasKey("player_level"))
        {
            Debug.Log("[LoadQuick] Không có save để load.");
            return;
        }

        if (levelSys == null) levelSys = FindAnyObjectByType<PlayerLevelSystem>();
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (equip == null) equip = FindAnyObjectByType<EquipmentManager>();
        if (weaponDB == null) weaponDB = FindAnyObjectByType<WeaponDatabase>();

        if (levelSys == null || player == null)
        {
            Debug.LogError("[LoadQuick] Thiếu tham chiếu Player/LevelSystem.");
            return;
        }

        var stats = levelSys.playerStats;

        // --- Core stats ---
        stats.level = SaveSystem.GetInt("player_level", stats.level);
        stats.exp = SaveSystem.GetFloat("player_exp", stats.exp);
        stats.expToNextLevel = SaveSystem.GetFloat("player_expToNext", stats.expToNextLevel);
        stats.statPoints = SaveSystem.GetInt("player_statPoints", stats.statPoints);

        stats.maxHealth = SaveSystem.GetFloat("player_hpMax", stats.maxHealth);
        stats.currentHealth = SaveSystem.GetFloat("player_hp", stats.currentHealth);
        stats.maxMana = SaveSystem.GetFloat("player_manaMax", stats.maxMana);
        stats.currentMana = SaveSystem.GetFloat("player_mana", stats.currentMana);


        // --- Vị trí ---
        Vector3 pos = SaveSystem.GetVector3("player_pos", player.position);
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.position = pos;
        if (cc != null) cc.enabled = true;
        Debug.Log($"[LoadQuick] Set player pos = {pos}");

        // --- Túi đồ ---
        if (Inventory.Instance != null && weaponDB != null)
        {
            string invJson = SaveSystem.GetString("inventory_json", "");
            if (!string.IsNullOrEmpty(invJson))
            {
                var invSave = JsonUtility.FromJson<Inventory.InventorySave>(invJson);
                Inventory.Instance.LoadFrom(invSave, weaponDB);
            }
        }

        // --- Trang bị đang mặc ---
        if (equip != null && Inventory.Instance != null)
        {
            string eqJson = SaveSystem.GetString("equipped_json", "");
            if (!string.IsNullOrEmpty(eqJson))
            {
                var eqSave = JsonUtility.FromJson<EquipmentManager.EquippedSave>(eqJson);
                equip.LoadEquippedFrom(eqSave, Inventory.Instance);
            }
        }
        if (PlayerWallet.Instance != null)
        {
            int savedCoin = SaveSystem.GetInt("player_coin", PlayerWallet.Instance.Coin);
            PlayerWallet.Instance.SetCoin(savedCoin);
            Debug.Log($"[LoadQuick] Load coin = {savedCoin}");
        }
        // Cập nhật lại chỉ số phụ & UI sau khi apply
        levelSys.RecalculateDerivedStats(true);
        levelSys.UpdateUI();
        Debug.Log("[LoadQuick] Load OK (stats + pos + inventory + equipped).");
    }
}
