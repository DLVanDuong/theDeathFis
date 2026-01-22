using UnityEngine;

public class IdleState : IState
{
    private EnemyStateMachine enemy;
    private float timer;
    private float waitTime = 4f; // Thời gian đứng yên trước khi đi tuần.

    public IdleState(EnemyStateMachine enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {               
        enemy.agent.isStopped = true; // Dừng NavMeshAgent khi vào trạng thái Idle
        if (enemy.enemyData.animationData != null)
        {
            enemy.animator.ResetTrigger(enemy.enemyData.animationData.attackTrigger); // Reset trigger Attack nếu có
            enemy.animator.SetFloat(enemy.enemyData.animationData.speedParam, 0f); // Đặt tốc độ về 0 trong Animator
            int idleIndex = Random.Range(0, 2);
            enemy.animator.SetInteger(enemy.enemyData.animationData.idleIndexParam, idleIndex); // Chọn ngẫu nhiên Idle Animation
            enemy.animator.SetTrigger(enemy.enemyData.animationData.playIdleActionTrigger); // Kích hoạt Animator Idle
        }
        else
        {
            enemy.animator.ResetTrigger("Attack"); // Reset trigger Attack nếu có
            enemy.animator.SetFloat("Speed", 0f); // Đặt tốc độ về 0 trong Animator
            enemy.animator.SetInteger("IdleIndex", Random.Range(0, 2)); // Chọn ngẫu nhiên Idle Animation
            enemy.animator.SetTrigger("PlayIdleAction"); // Kích hoạt Animator Idle
        }    
        timer = 0f; // Reset timer khi vào trạng thái Idle
    }
    public void Execute()
    {
        if (enemy.CanDetectPlayer())
        {
            enemy.ChangeState(enemy._states.Chase()); //
            return;
        }

        timer += Time.deltaTime;

        if (timer >= waitTime)
        {
            enemy.ChangeState(enemy._states.Patrol()); //
        }
    }
    public void Exit()
    {
        if(enemy.enemyData.animationData != null)
        {
            enemy.animator.ResetTrigger(enemy.enemyData.animationData.playIdleActionTrigger); // Reset trigger Idle nếu có
        }
        else
        {
            enemy.animator.ResetTrigger("PlayIdleAction"); // Reset trigger Idle nếu có
        }

    }
}
