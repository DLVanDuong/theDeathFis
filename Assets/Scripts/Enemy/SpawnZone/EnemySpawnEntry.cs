using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;       // Prefab enemy
    [Range(0, 100)] public int weight = 100; // Tỉ lệ spawn (càng cao càng dễ được chọn)
}
