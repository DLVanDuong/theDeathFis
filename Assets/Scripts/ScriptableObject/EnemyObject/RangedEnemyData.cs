using UnityEngine;

[CreateAssetMenu(fileName = "RangedEnemyData", menuName = "EnemyData/RangedEnemyData")]
public class RangedEnemyData : EnemyData
{
    [Header("Ranged Attack Parameters")]
    public GameObject projectilePrefab; // Prefab của đạn

    public float projectileSpeed = 10f; // Tốc độ của đạn

    public string SpawnPointName = "ProjectileSpawnPoint"; // Tên của điểm spawn đạn

    public float optimalAttackDistance = 5f; // Khoảng cách tấn công tối ưu của kẻ địch

    public float minAttackDistance = 3f;

    public bool useGravityForProjectile = true;  
}
