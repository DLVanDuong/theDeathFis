// PlayerIdleState.cs
public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(PlayerStateMachine stateMachine, PlayerStateFactory playerStateFactory)
        : base(stateMachine, playerStateFactory) { }

    public override void EnterState()
    {
        // Khi vào trạng thái Idle, đảm bảo các thông số animation di chuyển bằng 0
        Ctx.animatorManager.UpdateMovementParameters(0, 0, Ctx.isSprinting);
    }

    public override void UpdateState()
    {
        base.UpdateState(); // Cập nhật trạng thái hiện tại
        if(Ctx.CurrentMoveInput.x != 0 || Ctx.CurrentMoveInput.y != 0)
        {
            // Nếu có đầu vào di chuyển, chuyển sang trạng thái Move
            SwitchState(Factory.Move());
        }       
    }
    public override void ExitState() { base.ExitState(); } 
}