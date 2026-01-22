using UnityEngine;

public class AOEHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 50;   // damage gốc
    public float radius = 3f;

    [Header("Life Time")]
    public float lifeTime = 2f;

    [Header("Mode")]
    public bool instantDamage = true;
    public float tickInterval = 1f;

    private float nextTickTime;

    // thêm tham chiếu
    private EnemyStateMachine enemySM;

    private void Awake()
    {
        enemySM = GetComponentInParent<EnemyStateMachine>();
    }

    private void Start()
    {
        if (instantDamage)
        {
            ApplyDamage();
        }

        nextTickTime = Time.time + tickInterval;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!instantDamage && Time.time >= nextTickTime)
        {
            ApplyDamage();
            nextTickTime = Time.time + tickInterval;
        }
    }

    private void ApplyDamage()
    {
        int finalDamage = (enemySM != null)
            ? enemySM.GetDamage()      // <-- luôn lấy scaledDamage theo level
            : damage;                  // fallback nếu test rời rạc

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent<HealthPlayer>(out var hp))
            {
                hp.TakeDamage(finalDamage);
                AudioManager.Instance?.PlaySFXShort(AudioManager.Instance.playerHit, 0.4f, 1f);

                if (Resources.Load<GameObject>("BloodFX") is GameObject bloodFX)
                {
                    var fx = Instantiate(bloodFX, hit.transform.position + Vector3.up * 1.2f, Quaternion.identity);
                    Destroy(fx, 1.5f);
                }              
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
