using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IState
{
    private EnemyStateMachine enemy;
    private float time;
    private float chaseDuration = 5f;

    public ChaseState(EnemyStateMachine enemy) { this.enemy = enemy; }

    public void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.animator.applyRootMotion = false;
        enemy.agent.speed = enemy.enemyData.runSpeed;
        time = 0f;
    }

    public void Execute()
    {
        time += Time.deltaTime;

        if (enemy.player == null || time >= chaseDuration || !enemy.CanDetectPlayer())
        {
            enemy.ChangeState(enemy._states.Patrol());
            return;
        }

        // player chạy ra khỏi zone -> không đuổi nữa
        if (enemy.myZone != null && !enemy.myZone.Contains(enemy.player.position))
        {
            enemy.ChangeState(enemy._states.Patrol());
            return;
        }

        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);

        // luôn nhìn player
        Vector3 look = enemy.player.position; look.y = enemy.transform.position.y;
        enemy.transform.LookAt(look);

        var rangedData = enemy.enemyData as RangedEnemyData; // dữ liệu ranged của bạn
        if (rangedData != null)
        {
            if (dist < rangedData.minAttackDistance)
            {
                // lùi ra nhưng vẫn trong zone
                Vector3 dir = (enemy.player.position - enemy.transform.position).normalized;
                Vector3 raw = enemy.player.position - dir * rangedData.optimalAttackDistance;
                Vector3 target = enemy.ClampToZone(raw);

                NavMeshPath path = new NavMeshPath();
                if (enemy.agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    enemy.agent.isStopped = false;
                    enemy.agent.SetDestination(target);
                }
                else
                {
                    enemy.agent.isStopped = true;
                    Debug.LogWarning("Không tìm được vị trí hợp lệ để giữ khoảng cách (ranged).");
                }
            }
            else if (dist >= rangedData.minAttackDistance && dist <= rangedData.attackRange)
            {
                enemy.agent.isStopped = true;
                enemy.ChangeState(enemy._states.RangedAttack());
                return;
            }
            else
            {
                Vector3 target = enemy.ClampToZone(enemy.player.position);
                enemy.agent.isStopped = false;
                enemy.agent.SetDestination(target);
            }
        }
        else
        {
            if (dist <= enemy.enemyData.attackRange)
            {
                enemy.agent.isStopped = true;
                enemy.ChangeState(enemy._states.Attack());
                return;
            }
            else
            {
                Vector3 target = enemy.ClampToZone(enemy.player.position);
                enemy.agent.isStopped = false;
                enemy.agent.SetDestination(target);
            }
        }
    }

    public void Exit()
    {
        enemy.agent.speed = enemy.enemyData.speed;
        enemy.agent.isStopped = false;
    }
}
