using UnityEngine;

public class SaveQuick : MonoBehaviour
{
    [SerializeField] PlayerLevelSystem levelSys;
    [SerializeField] Transform player;
    [SerializeField] EquipmentManager equip;
    [SerializeField] WeaponDatabase weaponDB; // DB map saveKey -> WeaponData

    public void SaveNow()
    {
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
            Debug.LogError("[SaveQuick] Thiếu tham chiếu Player/LevelSystem.");
            return;
        }

        var stats = levelSys.playerStats;

        // --- Core stats ---
        SaveSystem.SetInt("player_level", stats.level);
        SaveSystem.SetFloat("player_exp", stats.exp);
        SaveSystem.SetFloat("player_expToNext", stats.expToNextLevel);
        SaveSystem.SetInt("player_statPoints", stats.statPoints);

        SaveSystem.SetFloat("player_hp", stats.currentHealth);
        SaveSystem.SetFloat("player_hpMax", stats.maxHealth);
        SaveSystem.SetFloat("player_mana", stats.currentMana);
        SaveSystem.SetFloat("player_manaMax", stats.maxMana);

        // --- Vị trí ---
        SaveSystem.SetVector3("player_pos", player.position);
        Debug.Log($"[SaveQuick] Save player_pos = {player.position}");

        // --- Túi đồ ---
        if (Inventory.Instance != null)
        {
            var inv = Inventory.Instance;
            var invSave = new Inventory.InventorySave();

            // 1️⃣ Lưu toàn bộ vũ khí đang nằm trong túi
            foreach (var w in inv.weapons)
            {
                if (w != null)
                    invSave.weapons.Add(w.ToSave());
            }

            // 2️⃣ Lưu thêm tất cả WeaponInstance đang được trang bị (nếu chưa có trong list)
            if (equip != null)
            {
                foreach (var kv in equip.GetAllEquippedInstances())
                {
                    var inst = kv.Value;
                    if (inst == null) continue;

                    // đảm bảo có instanceId
                    if (string.IsNullOrEmpty(inst.instanceId))
                        inst.instanceId = System.Guid.NewGuid().ToString();

                    bool exists = invSave.weapons.Exists(ws => ws.instanceId == inst.instanceId);
                    if (!exists)
                        invSave.weapons.Add(inst.ToSave());
                }
            }

            string invJson = JsonUtility.ToJson(invSave);
            SaveSystem.SetString("inventory_json", invJson);
        }

        // --- Trang bị đang mặc (map slot -> instanceId) ---
        if (equip != null)
        {
            var eqSave = equip.ToSaveEquipped();

            // lọc mấy entry rác nếu có
            for (int i = eqSave.slots.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(eqSave.instanceIds[i]))
                {
                    eqSave.slots.RemoveAt(i);
                    eqSave.instanceIds.RemoveAt(i);
                }
            }

            string eqJson = JsonUtility.ToJson(eqSave);
            SaveSystem.SetString("equipped_json", eqJson);
        }
        if (PlayerWallet.Instance != null)
        {
            SaveSystem.SetInt("player_coin", PlayerWallet.Instance.Coin);
            Debug.Log($"[SaveQuick] Save coin = {PlayerWallet.Instance.Coin}");
        }
    }
}
