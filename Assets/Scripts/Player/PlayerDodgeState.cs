using UnityEngine;

public class PlayerDodgeState : PlayerGroundedState
{
    private Vector3 dodgeDirection;

    private readonly float dodgeDuration = 0.4f; // Thời gian né
    private readonly float dodgeSpeed = 6f;
    private float dodgeTimer;

    public PlayerDodgeState(PlayerStateMachine stateMachine, PlayerStateFactory playerStateFactory)
        : base(stateMachine, playerStateFactory) { }

    public override void EnterState()
    {
        base.EnterState();
        Ctx.isDodgePressed = false; 
        dodgeTimer = dodgeDuration;

        Vector2 moveInput = Ctx.CurrentMoveInput;

        if(moveInput == Vector2.zero)
        {
            Ctx.animatorManager.UpdateMovementParameters(0, -1, false);
            dodgeDirection = -Ctx.transform.forward; // Né lùi nếu không có đầu vào di chuyển
        }
        else
        {
            Ctx.animatorManager.UpdateMovementParameters(moveInput.x, moveInput.y, false);
            Vector3 moveDirection3D = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            dodgeDirection = Ctx.transform.TransformDirection(moveDirection3D);
        }
        Ctx.animatorManager.CrossFadeInFixedTime(Ctx.animatorManager.DodgeBlendTreeHash, 0.1f);
    }
    public override void UpdateState()
    {
        dodgeTimer -= Time.deltaTime;
        Ctx.characterController.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
        if (dodgeTimer <= 0)
        {
            CheckSwitchStates(); // Kiểm tra điều kiện chuyển trạng thái sau khi né xong            
        }     
    }
    public override void ExitState() { base.ExitState(); }

    public override void CheckSwitchStates()
    {
        SwitchState(Factory.Idle()); // Sau khi né xong, chuyển về trạng thái Idle
    }
}
