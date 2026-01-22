using UnityEngine;

public class PlayerJumpState : PlayerAirborneState 
{  
    private const float CrossFadeDuration = 0.1f;
    public PlayerJumpState(PlayerStateMachine stateMachine, PlayerStateFactory playerStateFactory)
        : base(stateMachine, playerStateFactory) { }
    public override void EnterState()
    {
        base.EnterState();
        Ctx.isJumpPressed = false;
        Ctx.animatorManager.CrossFade(Ctx.animatorManager.JumpHash, CrossFadeDuration);

        // Áp dụng lực nhảy 
        Ctx.playerVelocityY = Ctx.jumpForce;
    }
    public override void UpdateState()
    {
        base.UpdateState(); // Gọi Update của lớp cha xem đã chạm đất chưa

    }
    public override void ExitState() { base.ExitState(); }
    
}
