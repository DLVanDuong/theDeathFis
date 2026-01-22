using UnityEngine;

public class BossHitboxManager : MonoBehaviour
{
    private EnemyHitbox[] hitboxes;

    private void Awake()
    {
        hitboxes = GetComponentsInChildren<EnemyHitbox>();

        if (hitboxes.Length == 0)
        {
           
        }
        else
        {
            
            foreach (var hb in hitboxes)
            {
                
            }
        }
    }

    // ===== Animation Event không tham số =====
    public void EnableHitbox()
    {
        EnableAll();
       
    }

    public void DisableHitbox()
    {
        DisableAll();
        
    }

    // ===== Animation Event có tham số =====
    public void EnableHitbox(string name)
    {
        foreach (var hitbox in hitboxes)
        {
            if (hitbox.gameObject.name.ToLower().Contains(name.ToLower()))
            {
                hitbox.EnableHitbox();
               
            }
        }
    }

    public void DisableHitbox(string name)
    {
        foreach (var hitbox in hitboxes)
        {
            if (hitbox.gameObject.name.ToLower().Contains(name.ToLower()))
            {
                hitbox.DisableHitbox();
               
            }
        }
    }

    public void EnableAll()
    {
        foreach (var hitbox in hitboxes) hitbox.EnableHitbox();
    }

    public void DisableAll()
    {
        foreach (var hitbox in hitboxes) hitbox.DisableHitbox();
    }
}
