// PlayerBowAttackState.cs
using UnityEngine;

public class PlayerBowAttackState : PlayerBaseState
{
    private readonly float attackDuration = 1.0f; // Thời gian animation bắn cung
    private float attackTimer;

    public PlayerBowAttackState(PlayerStateMachine stateMachine, PlayerStateFactory playerStateFactory)
        : base(stateMachine, playerStateFactory) { }

    public override void EnterState()
    {
        // Khi vào trạng thái, reset cờ nhấn nút tấn công và bắt đầu đếm thời gian
        Ctx.IsAttackPressed = false;
        attackTimer = attackDuration;

        // Kích hoạt animation tấn công
        Ctx.animatorManager.PlayTrigger(Ctx.animatorManager.AttackHash);

        // Hướng nhân vật về phía camera
        Ctx.HandleRotation();
    }

    public override void UpdateState()
    {
        attackTimer -= Time.deltaTime;
        Ctx.HandleRotation();
        Ctx.HandleAnimation();
        if (attackTimer <= 0f)
        {
            CheckSwitchStates();
        }
    }

    public override void ExitState()
    {
        // Khi thoát trạng thái, đảm bảo trigger tấn công được reset
        Ctx.animatorManager.ResetTrigger(Ctx.animatorManager.AttackHash);
    }

    public override void CheckSwitchStates()
    {
        // Sau khi bắn xong, luôn chuyển về trạng thái Idle để người chơi có thể di chuyển
        SwitchState(Factory.Idle());
    }
}