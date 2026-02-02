using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private int expReward = 20;
    public float damagePopupHeight = 1.0f;

    [Tooltip("Hệ số EXP khi là Boss")]
    public float bossExpMultiplier = 2f;
   
    public enum EnemyType { Flesh, Bone }
    public EnemyType enemyType = EnemyType.Flesh; // ✅ chọn trong Inspector

    private EnemyStateMachine enemyStateMachine;
    public bool isBoss = false;

    private int spawnLevelOverride = 0;

    [Header("Coin Drop")]
    public GameObject coinPrefab;  // kéo CoinPrefab vào đây
    public int minCoin = 1;
    public int maxCoin = 10;

    [Header("Stone Drop")]
    public GameObject stone0to5Prefab;
    public GameObject stone5to10Prefab;

    [Range(0, 100)] public int stone0to5Chance = 25;
    [Range(0, 100)] public int stone5to10Chance = 10;

    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action OnDied;

    public void SetSpawnLevel(int lvl) => spawnLevelOverride = Mathf.Max(1, lvl);

    private PlayerLevelSystem levelSystem;
    private bool isDead;

    [System.Serializable]
    public class DropEntry
    {
        [Header("Danh sách item có thể rớt trong nhóm này")]
        public List<ScriptableObject> itemDatas = new List<ScriptableObject>(); // nhiều item

        [Range(0, 100)]
        public int dropChance = 100;  // % xác suất nhóm này được chọn
    }

    [Header("Loot Settings")]
    public List<DropEntry> dropTable;
    public WeaponDropManager weaponDropManager;   // có thể bỏ trống, sẽ auto find
    private Color rarityColor;

    private int GetLevelForExp()
    {
        // Ưu tiên số Spawner truyền vào; nếu chưa có thì fallback qua StateMachine; cuối cùng là 1
        if (spawnLevelOverride > 0) return spawnLevelOverride;
        if (enemyStateMachine != null) return Mathf.Max(1, enemyStateMachine.Level);
        return 1;
    }
    private void Awake()
    {
        enemyStateMachine = GetComponent<EnemyStateMachine>();
        levelSystem = FindAnyObjectByType<PlayerLevelSystem>();
        if (levelSystem == null)
            EnsureDropManager();

    }
    private void Start()
    {
        int lvl = GetLevelForExp();
        int exp = expReward * lvl;
        if (isBoss) exp = Mathf.RoundToInt(exp * bossExpMultiplier);

        // 👉 Thêm dòng này để scale Damage và Health đúng theo level
        if (enemyStateMachine != null)
        {
            enemyStateMachine.ApplyLevelScaling(lvl);

            OnHealthChanged?.Invoke(enemyStateMachine.currentHealth, enemyStateMachine.GetMaxHealth());
        }
    }


    private int GetLevelSafe()
    {
        int lvl = 1;
        if (enemyStateMachine != null)
        {
            var t = enemyStateMachine.GetType();

            // 1) Property "Level"
            var prop = t.GetProperty("Level");
            if (prop != null && prop.PropertyType == typeof(int))
            {
                object v = prop.GetValue(enemyStateMachine, null);
                if (v is int i) lvl = i;
            }

            // 2) Field phổ biến
            var f1 = t.GetField("Level");
            if (f1 != null && f1.FieldType == typeof(int))
            {
                object v = f1.GetValue(enemyStateMachine);
                if (v is int i) lvl = i;
            }
            var f2 = t.GetField("level");
            if (f2 != null && f2.FieldType == typeof(int))
            {
                object v = f2.GetValue(enemyStateMachine);
                if (v is int i) lvl = i;
            }
            var f3 = t.GetField("currentLevel");
            if (f3 != null && f3.FieldType == typeof(int))
            {
                object v = f3.GetValue(enemyStateMachine);
                if (v is int i) lvl = i;
            }
        }
        return Mathf.Max(1, lvl);
    }
    private int ComputeExpReward()
    {
        int lvl = GetLevelForExp();
        int exp = expReward * lvl;
        if (isBoss) exp = Mathf.RoundToInt(exp * bossExpMultiplier);
        return Mathf.Max(1, exp);
    }
    private void EnsureDropManager()
    {
        if (weaponDropManager == null)
        {
            weaponDropManager = FindAnyObjectByType<WeaponDropManager>();
            if (weaponDropManager == null)
                Debug.LogWarning("[EnemyHealth] Không tìm thấy WeaponDropManager trong scene.");
        }
    }
    public float CurrentHealth => enemyStateMachine != null ? enemyStateMachine.currentHealth : 0;
    public void TakeDamage(int damage)
    {
        if (isDead || enemyStateMachine == null) return;

        // Hiện popup damage
        DynamicTextManager.CreateTextStacked(transform, damage.ToString(), DynamicTextManager.playerDamageData, baseUpOffset: damagePopupHeight);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.enemyHit, 1f);

        // Gọi trừ máu trong stateMachine
        enemyStateMachine.TakeDamage(damage);

        OnHealthChanged?.Invoke(enemyStateMachine.currentHealth, enemyStateMachine.GetMaxHealth());

        if (enemyStateMachine.currentHealth <= 0)
        {
            OnDie();
        }
    }

    private bool enemyStateMachineEnemyDead()
    {
        // EnemyStateMachine có isDead private → mình check bằng máu
        return enemyStateMachine != null && enemyStateMachine.enemyData != null
            && enemyStateMachine.currentHealth <= 0;
    }

    private void OnDie()
    {
        if (isDead) return;
        isDead = true;

        if (AudioManager.Instance != null)
        {
            if (isBoss)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.bossDeath, 1f);
            else
                switch (enemyType)
                {
                    case EnemyType.Flesh: AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyDeathFlesh, 1f); break;
                    case EnemyType.Bone: AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyDeathBone, 1f); break;
                }
        }

        if (levelSystem != null)
            levelSystem.AddExperience(ComputeExpReward());

        OnDied?.Invoke(); // 🩸 Gọi sự kiện chết cho BossUI fade out
        DropLoot();
        DropCoin();
        DropUpgradeStones();
    }
    private void DropUpgradeStones()
    {
        Vector3 centerPos = SnapToGround(transform.position);

        // rơi đá 0->5
        if (stone0to5Prefab != null && Random.Range(0, 100) < stone0to5Chance)
        {
            Vector3 pos = centerPos + new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.6f, 0.6f));
            var obj = Instantiate(stone0to5Prefab, SnapToGround(pos), Quaternion.identity);

            var pickup = obj.GetComponent<UpgradeStonePickup>();
            if (pickup != null) pickup.isDroppedFromEnemy = true;
        }

        // rơi đá 5->10 (boss rơi nhiều hơn)
        int chance = isBoss ? stone5to10Chance * 2 : stone5to10Chance;

        if (stone5to10Prefab != null && Random.Range(0, 100) < chance)
        {
            Vector3 pos = centerPos + new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.6f, 0.6f));
            var obj = Instantiate(stone5to10Prefab, SnapToGround(pos), Quaternion.identity);

            var pickup = obj.GetComponent<UpgradeStonePickup>();
            if (pickup != null) pickup.isDroppedFromEnemy = true;
        }
    }

    private void DropLoot()
    {
        if (dropTable == null || dropTable.Count == 0) return;

        Vector3 centerPos = transform.position; // tâm rơi
        centerPos = SnapToGround(centerPos);

        // ✅ chỉnh tại đây cho dễ
        float scatterRadius = 1.2f;     // bán kính tản ra
        float minSeparation = 0.6f;     // khoảng cách tối thiểu giữa các món
        List<Vector3> usedPositions = new List<Vector3>();

        int enemyLevel = GetLevelForExp(); // ✅ Lấy level enemy :contentReference[oaicite:1]{index=1}

        foreach (var entry in dropTable)
        {
            if (entry.itemDatas == null || entry.itemDatas.Count == 0) continue;

            int roll = Random.Range(0, 100);
            if (roll >= entry.dropChance) continue;

            ScriptableObject itemData = entry.itemDatas[Random.Range(0, entry.itemDatas.Count)];
            if (itemData == null) continue;

            // ✅ mỗi item có vị trí riêng (không dính)
            Vector3 dropPos = GetFreeDropPosition(centerPos, scatterRadius, minSeparation, usedPositions);

            // --- WEAPON ---
            if (itemData is WeaponData weaponData)
            {
                EnsureDropManager();

                WeaponInstance inst = weaponDropManager != null
                    ? weaponDropManager.GenerateRandomWeapon(weaponData, isBoss)
                    : new WeaponInstance(weaponData);

                if (isBoss)
                {
                    int rarityRoll = Random.Range(0, 100);
                    if (rarityRoll < 40) inst.rarity = WeaponRarity.Rare;
                    else if (rarityRoll < 80) inst.rarity = WeaponRarity.Epic;
                    else inst.rarity = WeaponRarity.Legendary;
                }

                inst.requiredLevel = enemyLevel;
                inst.damage += Mathf.RoundToInt(enemyLevel * 1.5f);

                inst.CaptureRolledBase();             

                if (weaponData.pickupPrefab != null)
                {
                    var obj = Instantiate(weaponData.pickupPrefab, dropPos, Quaternion.identity);

                    var pickup = obj.GetComponent<WeaponPickup>();
                    if (pickup != null)
                    {
                        pickup.isDroppedFromEnemy = true;
                        pickup.SetWeaponInstance(inst);
                    }

                    // ⭐ màu theo phẩm chất (giữ nguyên logic của bạn)
                    Color rarityColor = Color.white;
                    switch (inst.rarity)
                    {
                        case WeaponRarity.Common: rarityColor = Color.white; break;
                        case WeaponRarity.Rare: rarityColor = Color.cyan; break;
                        case WeaponRarity.Epic: rarityColor = new Color(0.7f, 0.2f, 1f); break;
                        case WeaponRarity.Legendary: rarityColor = new Color(1f, 0.8f, 0f); break;
                        case WeaponRarity.Mythic: rarityColor = new Color(1f, 0.5f, 0.2f); break;
                    }

                    var renderer = obj.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        Material mat = renderer.material;
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            mat.EnableKeyword("_EMISSION");
                            mat.SetColor("_EmissionColor", rarityColor * 1.5f);
                        }
                        else if (mat.HasProperty("_BaseColor"))
                        {
                            mat.SetColor("_BaseColor", rarityColor);
                        }
                        else
                        {
                            mat.color = rarityColor;
                        }
                    }

                    GameObject glow = new GameObject("RarityGlow");
                    glow.transform.SetParent(obj.transform);
                    glow.transform.localPosition = Vector3.up * 0.25f;

                    var light = glow.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.range = 2.2f;
                    light.intensity = 3.5f;
                    light.color = rarityColor;
                    light.shadows = LightShadows.None;

                    var aura = Resources.Load<GameObject>("DropAura");
                    if (aura != null)
                    {
                        var auraObj = Instantiate(aura, obj.transform.position + Vector3.up * 0.05f, Quaternion.Euler(90, 0, 0));
                        auraObj.transform.SetParent(obj.transform);
                        var auraRend = auraObj.GetComponentInChildren<Renderer>();
                        if (auraRend != null) auraRend.material.color = rarityColor;
                    }
                }

                continue;
            }

            // --- CONSUMABLE ---
            if (itemData is ConsumableData consumable && consumable.pickupPrefab != null)
            {
                Instantiate(consumable.pickupPrefab, dropPos, Quaternion.identity);
                continue;
            }

            // --- ARMOR ---
            if (itemData is ArmorData armor && armor.equipPrefab != null)
            {
                Instantiate(armor.equipPrefab, dropPos, Quaternion.identity);
                continue;
            }

            // --- RING ---
            if (itemData is RingData ring && ring.equipPrefab != null)
            {
                Instantiate(ring.equipPrefab, dropPos, Quaternion.identity);
                continue;
            }
        }
    }

    private Vector3 SnapToGround(Vector3 pos)
    {
        // Bám xuống Ground giống cách bạn đang làm
        if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("Ground")))
            return hit.point + Vector3.up * 0.3f;

        return pos;
    }

    private Vector3 GetFreeDropPosition(Vector3 center, float radius, float minSep, List<Vector3> used)
    {
        // thử tối đa 25 lần để tìm vị trí không đụng nhau
        for (int i = 0; i < 25; i++)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            Vector3 pos = center + new Vector3(r.x, 0f, r.y);

            bool ok = true;
            for (int j = 0; j < used.Count; j++)
            {
                if (Vector3.Distance(pos, used[j]) < minSep)
                {
                    ok = false;
                    break;
                }
            }
            if (!ok) continue;

            pos = SnapToGround(pos);
            used.Add(pos);
            return pos;
        }

        // fallback: nếu không tìm được, vẫn trả về center
        center = SnapToGround(center);
        used.Add(center);
        return center;
    }

    private void DropCoin()
    {
        if (coinPrefab == null) return;

        int amount = Random.Range(minCoin, maxCoin + 1);

        Vector3 spawnPos = transform.position + Vector3.up * 1.2f;
        if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("Ground")))
            spawnPos = hit.point + Vector3.up * 0.3f;

        var obj = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        var coin = obj.GetComponent<CoinPickup>();
        if (coin != null) coin.amount = amount;
    }

}
