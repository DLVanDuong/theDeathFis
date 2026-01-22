    // PlayerMoveState.cs
    public class PlayerMoveState : PlayerGroundedState
    {
        public PlayerMoveState(PlayerStateMachine stateMachine, PlayerStateFactory playerStateFactory)
            : base(stateMachine, playerStateFactory) { }

        public override void EnterState() { base.EnterState(); }

        public override void UpdateState()
        {
            base.UpdateState(); // Cập nhật trạng thái hiện tại

            // Cập nhật hướng xoay và animation của nhân vật
            Ctx.HandleRotation();
            Ctx.HandleAnimation();

            int weaponTypeID = Ctx.equipmentManager.CurrentWeaponType();
            Ctx.animatorManager.SetFloat(Ctx.animatorManager.WeaponTypeHash, weaponTypeID);

            // Chỉ kiểm tra logic riêng: khi nào thì dừng lại
            if(Ctx.CurrentMoveInput.x ==0 && Ctx.CurrentMoveInput.y == 0)
            {
                // Nếu không có đầu vào di chuyển, chuyển sang trạng thái Idle
                SwitchState(Factory.Idle());
                return;
            }
        }
        public override void ExitState() { base.ExitState(); }
    }