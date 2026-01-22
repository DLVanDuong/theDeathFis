using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage;

    private void OnTriggerEnter(Collider other)
    {
             
        if (other.CompareTag("Enemy"))
        {
           
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.enemyHit, 1f);

                var bloodPrefab = Resources.Load<GameObject>("BloodFX");
                if (bloodPrefab)
                {
                    GameObject fx = Instantiate(bloodPrefab, other.transform.position + Vector3.up * 1.2f, Quaternion.identity);
                    Destroy(fx, 1.5f);
                }
            }

            Destroy(gameObject); 
        }      
        else if (other.CompareTag("Player"))
        {
            HealthPlayer hp = other.GetComponent<HealthPlayer>();
            if (hp != null)
            {
                hp.TakeDamage(damage);

               
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.playerHit, 1f);

                
                var bloodPrefab = Resources.Load<GameObject>("BloodFX");
                if (bloodPrefab)
                {
                    GameObject fx = Instantiate(bloodPrefab, other.transform.position + Vector3.up * 1.2f, Quaternion.identity);
                    Destroy(fx, 1f);
                }
            }

            Destroy(gameObject);
        }
      
        else if (other.CompareTag("Ground"))
        {
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.swordSwing, 0.5f);
            Destroy(gameObject, 0.3f);
        }
    }
}

