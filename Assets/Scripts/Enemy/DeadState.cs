using UnityEngine;

public class DeadState : IState
{
    private EnemyStateMachine enemy;

    public DeadState(EnemyStateMachine enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        enemy.agent.isStopped = true; 
        enemy.agent.velocity = Vector3.zero; 
        enemy.animator.applyRootMotion = false;

        enemy.agent.isStopped = true;
        enemy.animator.applyRootMotion = false;

        if (enemy.enemyData.animationData != null)
            enemy.animator.SetTrigger(enemy.enemyData.animationData.dieTrigger);
        else
            enemy.animator.SetTrigger("Die");

        // Huỷ sau 2 giây (chờ animation)
        Object.Destroy(enemy.gameObject, 1f);
    }
    public void Execute()
    {
        // Không làm gì trong trạng thái chết
    }
    public void Exit()
    {
        // Không cần làm gì khi rời khỏi trạng thái chết
    }
}
