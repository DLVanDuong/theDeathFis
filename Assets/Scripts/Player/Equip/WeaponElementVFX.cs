using UnityEngine;

public class WeaponElementVFX : MonoBehaviour
{
    public Transform vfxAnchor;

    [Header("Element VFX Prefabs")]
    public GameObject vfxWindPrefab;
    public GameObject vfxThunderPrefab;
    public GameObject vfxFirePrefab;
    public GameObject vfxEarthPrefab;

    [Header("Offsets")]
    public Vector3 localPosOffset = Vector3.zero;
    public Vector3 localRotOffset = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    GameObject _wind, _thunder, _fire, _earth;
    Transform Anchor => vfxAnchor != null ? vfxAnchor : transform;

    public void Apply(WeaponInstance inst)
    {
        EnsureSpawned();

        if (inst == null || !inst.hasElementStone)
        {
            SetActive(false, false, false, false);
            return;
        }

        SetActive(
            inst.elementStone == UpgradeStoneType.Stone_Wind,
            inst.elementStone == UpgradeStoneType.Stone_Thunder,
            inst.elementStone == UpgradeStoneType.Stone_Fire,
            inst.elementStone == UpgradeStoneType.Stone_Earth
        );
    }

    void EnsureSpawned()
    {
        if (_wind == null && vfxWindPrefab) _wind = Spawn(vfxWindPrefab);
        if (_thunder == null && vfxThunderPrefab) _thunder = Spawn(vfxThunderPrefab);
        if (_fire == null && vfxFirePrefab) _fire = Spawn(vfxFirePrefab);
        if (_earth == null && vfxEarthPrefab) _earth = Spawn(vfxEarthPrefab);
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

    void SetActive(bool wind, bool thunder, bool fire, bool earth)
    {
        if (_wind) _wind.SetActive(wind);
        if (_thunder) _thunder.SetActive(thunder);
        if (_fire) _fire.SetActive(fire);
        if (_earth) _earth.SetActive(earth);
    }
}
