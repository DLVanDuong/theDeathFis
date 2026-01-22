using UnityEngine;

[CreateAssetMenu(fileName = "EliteEnemy", menuName = "EnemyData/EliteEnemyData")]
public class EliteEnemyData : EnemyData
{
    [Header("Elite Enemy Parameters")]
    public float specialAbilityCooldown = 6f; // Thời gian hồi chiêu của kỹ năng đặc biệt
    public int specialAbilityDamage = 20; // Sát thương của kỹ năng đặc biệt
    public string specialAtackTrigger = "SpecialAttack"; // Tên của trigger cho kỹ năng đặc biệt
}
