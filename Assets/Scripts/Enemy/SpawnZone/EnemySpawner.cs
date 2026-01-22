using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("Shared Zone")]
    public ZoneArea zone;

    [Header("Limits")]
    public int maxEnemies = 5;
    public float spawnInterval = 5f;

    [Header("Enemy Types (weighted)")]
    public List<EnemySpawnEntry> enemies;

    private readonly List<GameObject> spawned = new();
    private float nextSpawn;

    void Update()
    {
        spawned.RemoveAll(e => e == null);

        if (zone == null || enemies == null || enemies.Count == 0) return;
        if (spawned.Count >= maxEnemies) return;
        if (Time.time < nextSpawn) return;

        GameObject prefab = PickWeighted(enemies);
        if (!prefab) return;

        Vector3 pos = zone.SampleOnNavMesh(zone.GetRandomPoint(), 4f);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        spawned.Add(go);

        var esm = go.GetComponent<EnemyStateMachine>();
        if (esm != null)
        {
            esm.myZone = zone;
            int lvl = Random.Range(zone.minLevel, zone.maxLevel + 1);
            esm.ApplyLevelScaling(lvl);

            var eh = go.GetComponent<EnemyHealth>();
            if (eh != null) eh.SetSpawnLevel(lvl);
        }

        nextSpawn = Time.time + spawnInterval;
    }

    private static GameObject PickWeighted(List<EnemySpawnEntry> list)
    {
        int total = list.Sum(x => Mathf.Max(0, x.weight));
        if (total <= 0) return list[0].prefab;
        int roll = Random.Range(0, total);
        int acc = 0;
        foreach (var e in list)
        {
            acc += Mathf.Max(0, e.weight);
            if (roll < acc) return e.prefab;
        }
        return list[^1].prefab;
    }
}
