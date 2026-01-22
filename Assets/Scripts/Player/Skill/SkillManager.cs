using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("Quản lý kỹ năng")]
    public List<SkillData> availableSkills = new List<SkillData>();

    [Header("Kỹ năng mặc định")]
    public SkillData skill1; // Q
    public SkillData skill2; // E

    [Header("Tham chiếu")]
    private PlayerControls inputActions;
    private PlayerStatsRuntime playerStats;
    private HealthPlayer playerHealth;
    private PlayerStateMachine playerStateMachine;
    private Animator animator;
    private EquipmentManager equipmentManager;

    private float lastUseTime1 = 0f;
    private float lastUseTime2 = 0f;

    public SkillData currentSkill { get; set; }

    [Header("Spawn Point cho skill")]
    public Transform skillSpawnPoint;
    [SerializeField] private Transform weaponTransform;
    private Transform GetSafeSpawnPoint()
    {
        if (skillSpawnPoint != null) return skillSpawnPoint;
        if (weaponTransform != null) return weaponTransform;
        return this.transform;
    }

    [Header("Cooldown scale (ảnh hưởng bởi AGI)")]
    [Range(0.3f, 2f)] public float globalCooldownScale = 1f;

    void Awake()
    {
        inputActions = new PlayerControls();

        inputActions.Player.SkillQ.performed += ctx =>
        {
            if (skill1 != null) TryUseSkill(skill1, ref lastUseTime1);
        };
        inputActions.Player.SkillE.performed += ctx =>
        {
            if (skill2 != null) TryUseSkill(skill2, ref lastUseTime2);
        };

        var levelSystem = GetComponent<PlayerLevelSystem>();
        if (levelSystem != null)
            playerStats = levelSystem.playerStats;

        equipmentManager = GetComponent<EquipmentManager>();
        playerHealth = GetComponent<HealthPlayer>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
        animator = GetComponent<Animator>();
    }

    void OnEnable() => inputActions.Player.Enable();
    void OnDisable() => inputActions.Player.Disable();

    public void SetGlobalCooldownScale(float s) => globalCooldownScale = Mathf.Clamp(s, 0.3f, 2f);
    private float EffectiveCooldown(SkillData skill) => Mathf.Max(0f, skill.cooldown) * globalCooldownScale;

    private void TryUseSkill(SkillData skill, ref float lastUseTime)
    {
        if (skill == null) return;

        float cd = EffectiveCooldown(skill);
        if (Time.time < lastUseTime + cd) return;
        if (playerHealth.GetCurrentMana() < skill.manaCost) return;

        // Trừ mana & start CD
        playerHealth.RestoreMana(-skill.manaCost);
        lastUseTime = Time.time;
        currentSkill = skill;

        // SFX ▶ âm khi bấm chiêu (cast)
        AudioManager.Instance?.PlaySFX(skill.castSfx, skill.sfxVolume);

        animator.SetFloat("SkillIndex", skill.blendTreeIndex);
        animator.SetTrigger("CastSkill");

        // Đổi sang SkillState
        playerStateMachine.CurrentState
            .GetType()
            .GetMethod("SwitchState", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(playerStateMachine.CurrentState, new object[] { playerStateMachine.states.Skill(skill) });
    }

    // Animation Event sẽ gọi hàm này
    public void SpawnSkillEffect()
    {
        if (currentSkill == null) return;

        SkillData localSkill = currentSkill;
        var equipMgr = FindAnyObjectByType<EquipmentManager>();
        var bonus = equipMgr != null ? equipMgr.GetEquippedWeaponBonus() : default;

        int totalDamage = DamageCalculator.GetFinalDamage(playerStats, bonus, currentSkill);
        Debug.Log($"[SkillManager] Final Skill Damage = {totalDamage}");

        switch (localSkill.damageType)
        {
            case SkillDamageType.Single:
                SpawnSingle(localSkill, totalDamage, true); // phát 1 lần chắc chắn
                break;

            case SkillDamageType.MultiHit:
                StartCoroutine(SpawnMultiHit(localSkill, totalDamage));
                break;

            case SkillDamageType.HitThenAOE:
                StartCoroutine(SpawnHitThenAOE(localSkill, totalDamage));
                break;

            case SkillDamageType.Heal:
                playerStats.currentHealth = Mathf.Min(
                    playerStats.currentHealth + (int)localSkill.healAmount,
                    playerStats.maxHealth);
                if (localSkill.effectPrefab != null)
                {
                    Instantiate(localSkill.effectPrefab,
                        playerStateMachine.transform.position + Vector3.up * 1.5f,
                        Quaternion.identity);
                }
                break;

            case SkillDamageType.Buff:
                StartCoroutine(ApplyBuff(localSkill));
                break;
        }
    }

    private void SpawnSingle(SkillData skill, int totalDamage, bool playImpactSfx)
    {
        if (skill.effectPrefab == null) return;

        var obj = Instantiate(skill.effectPrefab, GetSpawnPosition(), GetSpawnRotation());
        var controller = obj.GetComponent<SkillEffectController>() ?? obj.AddComponent<SkillEffectController>();

        // SFX ▶ truyền clip impact cho effect (mỗi lần HIT sẽ phát)
        controller.Initialize(playerStats, totalDamage, 0f, 0.2f, skill.impactSfx, skill.sfxVolume);
    }

    private IEnumerator SpawnMultiHit(SkillData skill, int totalDamage)
    {
        for (int i = 0; i < skill.hitCount; i++)
        {
            // 1) PHÁT impact mỗi nhịp (đảm bảo đủ i lần dù không trúng)
            if (skill.impactSfx)
                AudioManager.Instance?.PlaySFX(skill.impactSfx, skill.sfxVolume);

            // 2) Spawn effect nhưng KHÔNG phát theo hit
            SpawnSingle(skill, totalDamage, false);

            yield return new WaitForSeconds(skill.delayBetweenHits);
        }
    }

    private IEnumerator SpawnHitThenAOE(SkillData skill, int totalDamage)
    {
        // 1) Nhát chém đầu: phát impact 1 lần chắc chắn
        if (skill.impactSfx)
            AudioManager.Instance?.PlaySFX(skill.impactSfx, skill.sfxVolume);

        // Spawn effect của nhát chém (không phát theo hit)
        SpawnSingle(skill, totalDamage, false);

        // 2) Đợi rồi nổ AOE
        yield return new WaitForSeconds(skill.aoeDelay);

        // Chọn clip nổ (fallback sang impact nếu thiếu)
        var aoeClip = skill.explosionSfx ? skill.explosionSfx : skill.impactSfx;

        // Phát nổ CHẮC CHẮN 1 lần
        if (aoeClip) AudioManager.Instance?.PlaySFX(aoeClip, skill.sfxVolume);

        // Spawn effect AOE (không phát theo hit)
        if (skill.aoeEffectPrefab != null)
        {
            var aoeObj = Instantiate(skill.aoeEffectPrefab, GetSpawnPosition(), Quaternion.identity);
            var controller = aoeObj.GetComponent<SkillEffectController>() ?? aoeObj.AddComponent<SkillEffectController>();
            controller.Initialize(playerStats, totalDamage, skill.aoeRadius, 0.5f, aoeClip, skill.sfxVolume, false);
        }
    }


    private Vector3 GetSpawnPosition()
    {
        // Ưu tiên SkillSpawnPoint do bạn gán trong Inspector
        if (skillSpawnPoint != null)
            return skillSpawnPoint.position;

        // Nếu có WeaponTransform gán riêng thì dùng
        if (weaponTransform != null)
            return weaponTransform.position;

        // Nếu có EquipmentManager thì tìm theo loại vũ khí đang cầm
        Transform point = GetWeaponSpawnPoint();
        if (point != null)
            return point.position + Vector3.up * 0.2f;

        // fallback nếu không có gì
        return transform.position + transform.forward * 1.5f + Vector3.up * 0.8f;
    }

    private Quaternion GetSpawnRotation()
    {
        // Ưu tiên SkillSpawnPoint do bạn gán
        if (skillSpawnPoint != null)
        {
            // Xoay theo hướng nhân vật, bỏ nghiêng tay
            Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);
            return rot;
        }

        // Nếu có WeaponTransform thì xoay theo hướng của nhân vật
        if (weaponTransform != null)
        {
            Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);
            return rot;
        }

        // Nếu có EquipmentManager thì lấy hướng của vũ khí chính
        Transform point = GetWeaponSpawnPoint();
        if (point != null)
        {
            Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);
            return rot;
        }

        // fallback
        return Quaternion.LookRotation(transform.forward, Vector3.up);
    }

    private IEnumerator ApplyBuff(SkillData skill)
    {
        playerStats.attackModifier = skill.statModifier;
        yield return new WaitForSeconds(skill.duration);
        playerStats.attackModifier = 1f;
    }

    public float GetCooldownRemaining(int slot)
    {
        if (slot == 1 && skill1 != null)
            return Mathf.Max(0, lastUseTime1 + EffectiveCooldown(skill1) - Time.time);
        if (slot == 2 && skill2 != null)
            return Mathf.Max(0, lastUseTime2 + EffectiveCooldown(skill2) - Time.time);
        return 0;
    }

    public void UnlockSkill(SkillData newSkill)
    {
        if (!availableSkills.Contains(newSkill))
            availableSkills.Add(newSkill);
    }

    public void EquipWeapon(WeaponInstance inst)
    {
        if (inst == null) return;
        skill1 = inst.skill1;
        skill2 = inst.skill2;
    }

    public void UnequipWeapon()
    {
        skill1 = null;
        skill2 = null;
    }
    private Transform GetWeaponSpawnPoint()
    {
        // Nếu có EquipmentManager → dùng hold point theo loại vũ khí
        if (equipmentManager != null)
        {
            int type = equipmentManager.CurrentWeaponType();

            // 🎯 Nếu là Bow (3) → bắn tay trái
            if (type == 3)
                return equipmentManager.GetWeaponHoldPointL();

            // 🎯 Nếu là Quiver (8) → tay phải (trang trí)
            if (type == 8)
                return equipmentManager.GetWeaponHoldPointR();

            // 🎯 Nếu là Shield (9) → tay trái (trang trí, không bắn)
            if (type == 9)
                return equipmentManager.GetWeaponHoldPointL();

            // 🎯 Mặc định → tay phải
            return equipmentManager.GetWeaponHoldPointR();
        }

        // fallback nếu chưa có manager
        return skillSpawnPoint != null ? skillSpawnPoint : transform;
    }
}