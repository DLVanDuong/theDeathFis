using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private int damage;
    private Collider hitboxCollider;
    private List<Collider> hitObjects = new List<Collider>();
    public float damagePopupHeight = 1.5f;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null)
        {
            
            return;
        }
        hitboxCollider.enabled = false;
    }

    public void SetDamage(int dmg)
    {
        this.damage = dmg;
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<EnemyHealth>(out var enemy))
        {
           
            enemy.TakeDamage(damage);
        }
    }

    public void EnableHitbox()
    {
        hitObjects.Clear(); 
        if (hitboxCollider != null)
            hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
    }
}
