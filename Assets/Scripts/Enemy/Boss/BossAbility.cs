using UnityEngine;

public class BossAbility : MonoBehaviour
{
    public string abilityName;       // tên của skill
    public int bonusDamage;              // sát thương
    public float range;              // phạm vi skill
    public GameObject vfxPrefab;     // particle prefab
    public Transform spawnPoint;     // vị trí spawn particle (nếu null thì fallback = transform boss)
    public AudioClip soundEffect;    // âm thanh skill

    public int abilityIndex;         // index để BlendTree chọn animation
    public AbilityType type;         // loại skill (Melee, Ranged, AOE...)

    [Header("Cooldown")]
    public float cooldown = 3f;                 // thời gian hồi skill
    [HideInInspector] public float lastCastTime = -999f;
    public int GetFinalDamage(EnemyStateMachine sm)
    {
        if (sm == null) return bonusDamage;
        return sm.GetDamage(bonusDamage);
        // scaledDamage (theo zone) + bonusDamage
    }
}

public enum AbilityType
{
    MeleeCombo,     // cận chiến
    RangedAttack,   // tầm xa
    AOE,            // diện rộng
    SummonMinions,  // triệu hồi
    Dash,           // lướt
    Stun            // làm choáng
}
