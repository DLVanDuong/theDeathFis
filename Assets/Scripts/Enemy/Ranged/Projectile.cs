using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    private int casterLayer;
    private Rigidbody rb;

    [SerializeField] private float lifeTime = 5f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Projectile prefab must have a Rigidbody component!");
        }
        Destroy(gameObject, 3);
    }

    public void Initialize(int dmg, float speed, int layer, Vector3 direction, bool useGravity) // Thêm tham số direction
    {
        this.damage = dmg;
        this.casterLayer = layer;
        gameObject.layer = casterLayer;

        if (rb != null)
        {
            rb.useGravity = useGravity;
            // Đặt velocity theo hướng và tốc độ
            rb.linearVelocity = direction * speed;
            // Đảm bảo mũi tên luôn hướng theo hướng di chuyển
            if (!useGravity)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        Destroy(gameObject, lifeTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthPlayer playerHealth = other.GetComponent<HealthPlayer>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);

                AudioManager.Instance?.PlaySFXShort(AudioManager.Instance.playerHit, 0.4f, 1f);

                if (Resources.Load<GameObject>("BloodFX") is GameObject bloodFX)
                {
                    var fx = Instantiate(bloodFX, other.transform.position + Vector3.up * 1.2f, Quaternion.identity);
                    Destroy(fx, 1f);
                }               
                Destroy(gameObject);
            }
        }        
    }
}