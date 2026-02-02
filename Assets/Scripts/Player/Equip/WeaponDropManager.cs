using UnityEngine;

public class WeaponDropManager : MonoBehaviour
{
    [Header("Pool skill đặc biệt cho Legendary")]
    public SkillData[] legendarySkills;

    [Header("Loot Label")]
    [SerializeField] private GameObject lootLabelPrefab; // kéo LootLabelCanvas.prefab vào
    [SerializeField] private Vector3 labelOffset = new Vector3(0, 1.2f, 0);

    public WeaponInstance GenerateRandomWeapon(WeaponData baseWeapon, bool isBoss = false)
    {
        WeaponInstance inst = new WeaponInstance(baseWeapon);

        int roll = Random.Range(0, 100);

        if (isBoss) // Boss → Rare trở lên
        {
            if (roll < 50) inst.rarity = WeaponRarity.Rare;        // 50%
            else if (roll < 85) inst.rarity = WeaponRarity.Epic;   // 35%
            else inst.rarity = WeaponRarity.Legendary;             // 15%
        }
        else // Quái thường → Common tới Epic
        {
            if (roll < 70) inst.rarity = WeaponRarity.Common;      // 70%
            else if (roll < 90) inst.rarity = WeaponRarity.Rare;   // 20%
            else inst.rarity = WeaponRarity.Epic;                  // 10%
        }

        ApplyRandomStats(inst);

        inst.CaptureRolledBase();

        if (inst.rarity == WeaponRarity.Legendary && legendarySkills != null && legendarySkills.Length > 0)
        {
            SkillData special = legendarySkills[Random.Range(0, legendarySkills.Length)];
            if (inst.skill1 == null) inst.skill1 = special;
            else inst.skill2 = special;
        }

        return inst;
    }

    private void ApplyRandomStats(WeaponInstance inst)
    {
        switch (inst.rarity)
        {
            case WeaponRarity.Common:
                inst.damage = Random.Range(inst.template.baseDamage - 2, inst.template.baseDamage + 5);
                inst.strength = Random.Range(1, 10);
                inst.agility = Random.Range(1, 10);
                inst.vitality = Random.Range(1, 10);
                inst.energy = Random.Range(1, 10);
                break;

            case WeaponRarity.Rare:
                inst.damage = Random.Range(inst.template.baseDamage + 2, inst.template.baseDamage + 10);
                inst.strength = Random.Range(4, 20);
                inst.agility = Random.Range(4, 20);
                inst.vitality = Random.Range(4, 20);
                inst.energy = Random.Range(4, 20);
                break;

            case WeaponRarity.Epic:
                inst.damage = Random.Range(inst.template.baseDamage + 6, inst.template.baseDamage + 50);
                inst.strength = Random.Range(7, 35);
                inst.agility = Random.Range(7, 35);
                inst.vitality = Random.Range(7, 35);
                inst.energy = Random.Range(7, 35);
                break;

            case WeaponRarity.Legendary:
                inst.damage = Random.Range(inst.template.baseDamage + 12, inst.template.baseDamage + 100);
                inst.strength = Random.Range(8, 45);
                inst.agility = Random.Range(8, 45);
                inst.vitality = Random.Range(8, 45);
                inst.energy = Random.Range(8, 45);
                break;
        }
    }

    public void SpawnWeaponDrop(WeaponInstance inst, Vector3 dropPos)
    {
        if (inst == null || inst.template == null) return;
        if (inst.template.pickupPrefab == null) return;

        GameObject dropObj = Instantiate(inst.template.pickupPrefab, dropPos, Quaternion.identity);

        // ✅ tránh WeaponPickup Awake tự disable
        dropObj.SetActive(false);

        var pickup = dropObj.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.isDroppedFromEnemy = true;
        }

        dropObj.SetActive(true);

        if (pickup != null)
        {
            pickup.SetWeaponInstance(inst); // ✅ hiện tên/phẩm/màu trong WeaponPickup
        }

        // ✅ Spawn LootLabelCanvas (Tên + phẩm) nếu bạn muốn label riêng
        if (lootLabelPrefab != null)
        {
            GameObject labelObj = Instantiate(lootLabelPrefab, dropObj.transform);
            labelObj.transform.localPosition = labelOffset;
            labelObj.transform.localRotation = Quaternion.identity;
            labelObj.transform.localScale = Vector3.one * 0.01f; // chống chữ quá to

            var lootLabel = labelObj.GetComponent<LootPlusLabel>();
            if (lootLabel != null) lootLabel.SetWeapon(inst);
        }
    }

}
