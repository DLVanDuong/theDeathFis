using UnityEngine;

public class WeaponElementVFX : MonoBehaviour
{
    [Header("Element VFX Objects")]
    public GameObject vfxWind;
    public GameObject vfxThunder;
    public GameObject vfxFire;
    public GameObject vfxEarth;

    [Header("Optional: auto stop particle when disabling")]
    public bool stopParticlesOnDisable = true;

    public void Apply(WeaponInstance inst)
    {
        if (inst == null || !inst.hasElementStone)
        {
            SetOnly(null);
            return;
        }

        switch (inst.elementStone)
        {
            case UpgradeStoneType.Stone_Wind: SetOnly(vfxWind); break;
            case UpgradeStoneType.Stone_Thunder: SetOnly(vfxThunder); break;
            case UpgradeStoneType.Stone_Fire: SetOnly(vfxFire); break;
            case UpgradeStoneType.Stone_Earth: SetOnly(vfxEarth); break;
            default: SetOnly(null); break;
        }
    }

    void SetOnly(GameObject target)
    {
        SetActiveSafe(vfxWind, target == vfxWind);
        SetActiveSafe(vfxThunder, target == vfxThunder);
        SetActiveSafe(vfxFire, target == vfxFire);
        SetActiveSafe(vfxEarth, target == vfxEarth);
    }

    void SetActiveSafe(GameObject go, bool on)
    {
        if (!go) return;

        if (!on && stopParticlesOnDisable)
        {
            // stop particles cleanly before hiding
            var ps = go.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < ps.Length; i++)
                ps[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        go.SetActive(on);
    }
}
