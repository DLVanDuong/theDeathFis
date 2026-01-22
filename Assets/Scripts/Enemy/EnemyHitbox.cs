using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private Collider attackCollider;
    public EnemyStateMachine enemyStateMachine;

    private void Awake()
    {
        attackCollider = GetComponent<Collider>(); //lấy collider của hitbox
        if (attackCollider == null)
        {
            Debug.LogError("Collider component missing on EnemyHitbox GameObject: " + gameObject.name);
            return;
        }
        attackCollider.isTrigger = true; // Đặt collider là trigger
        attackCollider.enabled = false; // Vô hiệu hóa hitbox ban đầu
        enemyStateMachine = GetComponentInParent<EnemyStateMachine>(); // Lấy EnemyStateMachine từ GameObject cha

    }
    private void OnTriggerEnter(Collider other)
    {
        //kiểm tra xem va chạm có phải là Player hay không
        if (other.CompareTag("Player"))
        {
            enemyStateMachine.HitboxConnectedWithPlayer(other); // Gọi phương thức trong EnemyStateMachine
            DisableHitbox(); // Vô hiệu hóa hitbox sau khi va chạm
        }

    }
    public void EnableHitbox()
    {
        
        attackCollider.enabled = true; // Kích hoạt hitbox
    }
    public void DisableHitbox()
    {
        
        attackCollider.enabled = false; // Vô hiệu hóa hitbox
    }
}
