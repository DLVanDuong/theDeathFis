using UnityEngine;
using UnityEngine.Scripting;
using System.Collections;

[Preserve] // ⚙️ Giữ class lại khi build IL2CPP
public class SkillEffectController : MonoBehaviour
{
    private int damage;
    private float radius;
    private float activeTime = 0.2f;

    // SFX nhận từ SkillManager
    private AudioClip impactSfx;
    private float impactVolume = 1f;
    private bool playSfxOnHit = false;

    private Collider col;

    void Start()
    {
        var ps = GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        else
            Destroy(gameObject, 1f);
    }

    public void Initialize(
        PlayerStatsRuntime stats,
        int calculatedDamage,
        float aoeRadius = 0f,
        float aoeActiveTime = 0.2f,
        AudioClip impact = null,
        float volume = 1f,
        bool playOnHit = false)
    {
        damage = calculatedDamage;
        radius = aoeRadius;
        activeTime = aoeActiveTime;

        impactSfx = impact;
        impactVolume = volume;
        playSfxOnHit = playOnHit;

        if (radius > 0f)
        {
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;
            col = sphere;

            var rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            StartCoroutine(EnableColliderTemporary());
        }
        else
        {
            col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
    }

    private IEnumerator EnableColliderTemporary()
    {
        if (col != null)
        {
            col.enabled = true;
            yield return new WaitForSeconds(activeTime);
            col.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out EnemyHealth enemy))
        {
            enemy.TakeDamage(damage);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[SkillEffect] Hit {enemy.name} damage={damage}");
#endif
        }

        if (playSfxOnHit && impactSfx)
            AudioManager.Instance?.PlaySFX(impactSfx, impactVolume);
    }

    public void SetDamage(int value)
    {
        damage = value;
    }
}
