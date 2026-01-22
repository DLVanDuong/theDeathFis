using System.Collections;
using UnityEngine;

public class RangedAttackState : IState
{
    private EnemyStateMachine enemy;
    private RangedEnemyData rangedData;

    public RangedAttackState(EnemyStateMachine enemy)
    {
        this.enemy = enemy;
        rangedData = enemy.enemyData as RangedEnemyData;
        if (rangedData == null)
        {
            Debug.LogError("RangedEnemyData không được gán cho EnemyStateMachine.");
            enemy.ChangeState(enemy._states.Idle());
        }
    }
    public void Enter()
    {
        if (rangedData == null) return;

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;
        enemy.animator.applyRootMotion = false;

        if (Time.time >= enemy.lastAttackTime + rangedData.attackCooldown)
        {
            enemy.StartCoroutine(PerformAttack());
        }
    }
    private IEnumerator PerformAttack()
    {
        enemy.StartAttackAnimation();
        yield return new WaitForSeconds(0.5f); // Chờ 1 giây để animator chạy
        enemy.ShootProjectile(rangedData.useGravityForProjectile);
    }
    public void Execute()
    {
        if (rangedData == null) return;

        Vector3 lookTarget = enemy.player.position;
        lookTarget.y = enemy.transform.position.y;
        enemy.transform.LookAt(lookTarget);

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (!enemy.isAttacking && (distanceToPlayer > rangedData.optimalAttackDistance || distanceToPlayer < rangedData.minAttackDistance))
        {
            enemy.ChangeState(enemy._states.Chase());
            return;
        }

        if (!enemy.isAttacking && Time.time >= enemy.lastAttackTime + rangedData.attackCooldown)
        {
            enemy.StartCoroutine(PerformAttack());
        }
    }

    public void Exit()
    {
        if (rangedData != null)
        {
            enemy.agent.isStopped = false;
        }
    }
}