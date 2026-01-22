using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IState
{
    private EnemyStateMachine enemy;

    public PatrolState(EnemyStateMachine enemy) { this.enemy = enemy; }

    public void Enter()
    {
        Vector3 patrolPoint;
        if (enemy.myZone != null)
        {
            patrolPoint = enemy.myZone.GetRandomPoint();
        }
        else
        {
            enemy.SearchWalkPoint();           // hàm này trả về void
            patrolPoint = enemy.randomPatrolTarget;
        }

        // Nhìn về hướng mục tiêu
        Vector3 lookTarget = patrolPoint;
        lookTarget.y = enemy.transform.position.y;
        enemy.transform.LookAt(lookTarget);

        // Reset, bật agent, tốc độ
        enemy.animator.ResetTrigger("PlayIdleAction");
        enemy.agent.isStopped = false;
        enemy.agent.speed = enemy.enemyData.speed;

        // Ép đích nằm trong zone + navmesh (nếu bạn đã có hàm này)
        Vector3 dest = enemy.ClampToZone(patrolPoint);
        enemy.agent.SetDestination(dest);
    }

    public void Execute()
    {
        if (enemy.CanDetectPlayer()) // sẵn có trong EnemyStateMachine
        {
            enemy.ChangeState(enemy._states.Chase());
            return;
        }

        // nếu có lỡ trôi ra biên
        if (enemy.myZone != null && !enemy.myZone.Contains(enemy.transform.position))
        {
            Vector3 back = enemy.ClampToZone(enemy.transform.position);
            enemy.agent.SetDestination(back);
        }

        if ((!enemy.agent.pathPending && enemy.agent.remainingDistance < 0.5f)
            || enemy.agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            enemy.ChangeState(enemy._states.Idle());
        }
    }

    public void Exit() { }
}
