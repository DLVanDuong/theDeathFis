using UnityEngine;

public class EnemyStateFactory
{
    private EnemyStateMachine _enemy;

    // Constructor để truyền EnemyStateMachine vào factory
    public EnemyStateFactory(EnemyStateMachine enemyContext)
    {
        _enemy = enemyContext;
    }

    // Các phương thức để tạo và trả về các trạng thái cụ thể
    public IState Idle()
    {
        return new IdleState(_enemy);
    }

    public IState Patrol()
    {
        return new PatrolState(_enemy);
    }

    public IState Chase()
    {
        return new ChaseState(_enemy);
    }

    public IState Attack()
    {
        return new AttackState(_enemy);
    }

    public IState RangedAttack()
    {
        return new RangedAttackState(_enemy);
    }

    public IState Dead()
    {
        return new DeadState(_enemy);
    }

    // Bạn có thể thêm các phương thức khác ở đây cho các trạng thái mới
    // Ví dụ: Stunned(), Flee(), ...
}