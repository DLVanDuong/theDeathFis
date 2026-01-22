using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine stateMachine, PlayerStateFactory playerStateFactory)
        : base(stateMachine, playerStateFactory) { }
    
    public override void EnterState() { }
    
    public override void UpdateState()
    {       
        CheckSwitchStates();
    }
    public override void ExitState() { }
    public override void CheckSwitchStates()
    {
        //nếu người chơi nhảy, chuyển sang trạng thái nhảy
        if (Ctx.isJumpPressed)
        {
            SwitchState(Factory.Jump());
            return;
        }
        //Nếu người chơi không nhấn nút tấn công, chuyển sang trạng thái tấn công
        if (Ctx.IsAttackPressed)
        {
            // Lấy ID của vũ khí hiện tại
            int weaponTypeID = Ctx.equipmentManager.CurrentWeaponType();
            // Sử dụng Factory để lấy đúng trạng thái tấn công
            SwitchState(Factory.GetAttackStateForWeapon(weaponTypeID));
            return;
        }
        // nếu người chơi đang di chuyển, chuyển sang trạng thái di chuyển
        if (Ctx.isDodgePressed)
        {
            SwitchState(Factory.Dodge());
            return;
        }
    }
}
