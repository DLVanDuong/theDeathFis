using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DB/WeaponDatabase")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponData> all;
    private Dictionary<string, WeaponData> map;

    public void Init()
    {
        if (map != null) return;
        map = new Dictionary<string, WeaponData>();
        foreach (var w in all)
        {
            if (w != null && !string.IsNullOrEmpty(w.saveKey))
                map[w.saveKey] = w;
        }
    }

    public WeaponData GetByKey(string key)
    {
        Init();
        return (!string.IsNullOrEmpty(key) && map.TryGetValue(key, out var v)) ? v : null;
    }
}
