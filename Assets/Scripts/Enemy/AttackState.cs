using UnityEngine;

public class AttackState : IState
{
    private EnemyStateMachine enemy;  
    public AttackState(EnemyStateMachine enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;
        enemy.animator.applyRootMotion = false;

        if (Time.time >= enemy.lastAttackTime + enemy.enemyData.attackCooldown)
        {
            var bossController = enemy.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.UseSmartAbility(); // gọi skill random
            }
            else
            {
                enemy.StartAttackAnimation(); // enemy thường vẫn dùng attack bình thường
            }
        }
    }

    public void Execute()
    {
        Vector3 lookTarget = enemy.player.position;
        lookTarget.y = enemy.transform.position.y;
        enemy.transform.LookAt(lookTarget);
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (!enemy.isAttacking && distanceToPlayer > enemy.enemyData.attackRange)
        {
            enemy.ChangeState(enemy._states.Chase());
            return;
        }

        if (!enemy.isAttacking && Time.time >= enemy.lastAttackTime + enemy.enemyData.attackCooldown)
        {
            var bossController = enemy.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.UseSmartAbility();
            }
            else
            {
                enemy.StartAttackAnimation();
            }
        }
    }
    public void Exit()
    {
        enemy.agent.isStopped = false;
    }
}