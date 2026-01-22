using UnityEngine;
using UnityEngine.Scripting;
// Định nghĩa một enum cho loại kỹ năng
public enum SkillDamageType
{
    // === Attack Skill ===
    Single,         // 1 hit duy nhất
    MultiHit,       // nhiều hit liên tiếp
    DamageOverTime, // damage theo thời gian
    HitThenAOE,     // trúng 1 hit rồi nổ AOE

    // === Non-Damage Skill ===
    Buff,           // Buff cho người chơi (tăng chỉ số)
    Debuff,         // Gây hiệu ứng bất lợi lên enemy
    Heal            // Hồi máu cho player
}

[CreateAssetMenu(fileName = "Skill", menuName = "Skill System/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string skillName; // Tên kỹ năng
    public SkillDamageType damageType; // Loại kỹ năng
    public Sprite skillIcon; // Biểu tượng kỹ năng

    [Header("Yêu cầu")]
    public int requiredLevel = 1; // Cấp độ yêu cầu để sử dụng kỹ năng

    [Header("Chỉ số")]
    public int manaCost;  // Mana tiêu hao khi sử dụng kỹ năng
    public float cooldown; // Thời gian hồi chiêu (giây)

    [Header("Attack Settings")]
    public float damage;
    public int hitCount = 1;
    public float delayBetweenHits = 0.2f;
    public float aoeDelay = 1f;
    public float aoeDamage;
    public float aoeRadius;  

    [Header("Buff / Debuff Settings")]
    public float duration;          // Thời gian hiệu lực
    public float statModifier = 1f; // Hệ số tăng/giảm
    public string targetStat;       // "Attack", "Defense", "Speed"...

    [Header("Heal Settings")]
    public float healAmount;

    [Header("Effect Prefabs")]
    public GameObject effectPrefab;     // Hiệu ứng chính
    public GameObject aoeEffectPrefab;  // Hiệu ứng AOE

    [Header("Audio")]
    public AudioClip castSfx;           // âm khi tung skill
    public AudioClip impactSfx;         // âm khi chem trúng
    public AudioClip explosionSfx;      // âm khi nổ AOE
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Animation")]
    public int blendTreeIndex; 
}