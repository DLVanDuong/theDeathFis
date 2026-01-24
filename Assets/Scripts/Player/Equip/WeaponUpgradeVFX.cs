using UnityEngine;

public class WeaponUpgradeVFX : MonoBehaviour
{
    [Header("Anchor (để trống thì dùng object này)")]
    public Transform vfxAnchor;

    [Header("VFX Prefabs")]
    public GameObject vfxPlus5Prefab;       // hình 1
    public GameObject vfxPlus10Prefab;      // hình 2
    public GameObject vfxRedWeaponPrefab;   // hình 3

    [Header("Đỏ là rarity nào?")]   
    public WeaponRarity redRarity = WeaponRarity.Mythic;

    [Header("Offsets")]
    public Vector3 localPosOffset = Vector3.zero;
    public Vector3 localRotOffset = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    GameObject _plus5;
    GameObject _plus10;
    GameObject _red;

    Transform Anchor => vfxAnchor != null ? vfxAnchor : transform;

    public void Apply(WeaponInstance inst)
    {
        if (inst == null)
        {
            SetActiveAll(false, false, false);
            return;
        }

        bool showPlus10 = inst.upgradeLevel >= 10;
        bool showPlus5 = inst.upgradeLevel >= 5 && inst.upgradeLevel < 10;

        // Ưu tiên +10
        if (showPlus10) showPlus5 = false;

        bool showRed = (inst.rarity == redRarity);

        EnsureSpawned();
        SetActiveAll(showPlus5, showPlus10, showRed);
    }

    void EnsureSpawned()
    {
        if (_plus5 == null && vfxPlus5Prefab != null) _plus5 = Spawn(vfxPlus5Prefab);
        if (_plus10 == null && vfxPlus10Prefab != null) _plus10 = Spawn(vfxPlus10Prefab);
        if (_red == null && vfxRedWeaponPrefab != null) _red = Spawn(vfxRedWeaponPrefab);
    }

    GameObject Spawn(GameObject prefab)
    {
        var go = Instantiate(prefab, Anchor);
        go.transform.localPosition = localPosOffset;
        go.transform.localEulerAngles = localRotOffset;
        go.transform.localScale = localScale;
        go.SetActive(false);
        return go;
    }

    void SetActiveAll(bool plus5, bool plus10, bool red)
    {
        if (_plus5) _plus5.SetActive(plus5);
        if (_plus10) _plus10.SetActive(plus10);
        if (_red) _red.SetActive(red);
    }
}
