using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class EliteAttackState : IState
{
    private EnemyStateMachine enemy;
    private float normalAttackTimer;
    private float specialAttackTimer;
    private bool isAttacking = false;
    private EliteEnemyData eliteData;

    public EliteAttackState(EnemyStateMachine enemy)
    {
        this.enemy = enemy;
        eliteData = enemy.enemyData as EliteEnemyData; // Lấy dữ liệu riêng của kẻ địch Elite
        if (eliteData == null)
        {
            Debug.LogError("EliteEnemyData không được gán cho EnemyStateMachine.");
            eliteData = ScriptableObject.CreateInstance<EliteEnemyData>();
        }
    }
    public void Enter()
    {
        enemy.agent.isStopped = true; // Dừng di chuyển khi vào trạng thái tấn công
        enemy.agent.velocity = Vector3.zero; // Reset vận tốc của NavMeshAgent
        enemy.animator.applyRootMotion = false; // Tắt root motion để có thể điều khiển vị trí của kẻ địch

        normalAttackTimer = 0f; // Reset thời gian đòn đánh thường
        specialAttackTimer = 0f; // Reset thời gian đòn đánh đặc biệt
        StartNormalAttack(); // Bắt đầu bằng đòn đánh thường
    }
    private void StartNormalAttack()
    {
        isAttacking = true;
        if (enemy.enemyData.animationData != null)
        {
            enemy.animator.SetTrigger(enemy.enemyData.animationData.attackTrigger);
        }
        else
        {
            enemy.animator.SetTrigger("Attack");
        }
        // Logic kích hoạt hitbox hoặc gây sát thương (thông qua Animation Event hoặc sự kiện)
    }
    private void StartSpecialAttack()
    {
        isAttacking = true;
        if (eliteData.specialAtackTrigger != "")
        {
            enemy.animator.SetTrigger(eliteData.specialAtackTrigger);
        }
        else if (enemy.enemyData.animationData != null)
        {
            enemy.animator.SetTrigger(enemy.enemyData.animationData.attackTrigger); // Fallback nếu không có trigger riêng
        }
        else
        {
            enemy.animator.SetTrigger("Attack");
        }
        // Logic gây sát thương cho kỹ năng đặc biệt (có thể khác với đòn thường)
        // Ví dụ: gây sát thương AOE, hoặc một đòn đánh mạnh hơn
    }
    public void Execute() 
    {
        normalAttackTimer += Time.deltaTime;
        specialAttackTimer += Time.deltaTime;

        // Luôn nhìn về phía người chơi
        Vector3 lookTarget = enemy.player.position;
        lookTarget.y = enemy.transform.position.y; // Giữ y để không nhìn lên/xuống
        enemy.transform.LookAt(lookTarget);

        if(Vector3.Distance(enemy.transform.position, enemy.player.position) > eliteData.attackRange)
        {
            enemy.ChangeState(new ChaseState(enemy)); // Nếu ra ngoài tầm tấn công, chuyển sang trạng thái Chase
            return;
        }
        if(specialAttackTimer >= eliteData.specialAbilityCooldown && !isAttacking)
        {
            StartSpecialAttack(); // Bắt đầu đòn đánh đặc biệt nếu đủ thời gian hồi chiêu
            specialAttackTimer = 0f; // Reset thời gian hồi chiêu
            normalAttackTimer = 0f; // Reset thời gian đòn đánh thường
            return;
        }else if(normalAttackTimer >= eliteData.attackCooldown && !isAttacking)
        {
            StartNormalAttack(); // Bắt đầu đòn đánh thường nếu đủ thời gian hồi chiêu
            normalAttackTimer = 0f; // Reset thời gian đòn đánh thường          
        }
    }
    public void Exit()
    {
        isAttacking = false; // Reset trạng thái tấn công khi thoát khỏi trạng thái này
        enemy.agent.isStopped = false; // Bật lại NavMeshAgent sau khi kết thúc tấn công
    }
}
