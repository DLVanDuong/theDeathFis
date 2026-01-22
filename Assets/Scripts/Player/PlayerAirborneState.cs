using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(PlayerStateMachine stateMachine, PlayerStateFactory playerStateFactory)
        : base(stateMachine, playerStateFactory) { }
    public override void EnterState() { }
    public override void UpdateState()
    {
        // Cập nhật trạng thái nhảy, có thể bao gồm kiểm tra va chạm, cập nhật vị trí, v.v.
        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void CheckSwitchStates()
    {
       //Nếu nhân vật chạm mặt đất, chuyển sang trạng thái đứng
        if(Ctx.characterController.isGrounded)
        {
            SwitchState(Factory.Idle());
            return;
        }
    }
}
