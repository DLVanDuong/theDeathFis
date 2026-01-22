// PlayerStateFactory.cs
using System;
using System.Collections.Generic;

public class PlayerStateFactory
{
    private PlayerStateMachine _context;
    private Dictionary<int, Func<PlayerBaseState>> attackStates;


    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        // Khởi tạo từ điển và ánh xạ các ID vũ khí với các trạng thái tương ứng
        attackStates = new Dictionary<int, Func<PlayerBaseState>>
        {
            { 0, () => new PlayerAttackState(_context, this) }, // 0: Không vũ trang
            { 1, () => new PlayerAttackState(_context, this) }, // 1: Kiếm
            { 2, () => new PlayerAttackState(_context, this) }, // 2: Rìu
            { 3, () => new PlayerBowAttackState(_context, this) } // 3: Cung
            // Bạn có thể thêm các loại vũ khí khác ở đây
        };
    }

    // Các phương thức để tạo và trả về một state cụ thể
    public PlayerBaseState Idle()
    {
        return new PlayerIdleState(_context, this);
    }
    public PlayerBaseState Move()
    {
        return new PlayerMoveState(_context, this);
    }
    public PlayerBaseState Attack()
    {
        return new PlayerAttackState(_context, this);
    }
    // Bạn có thể thêm các state khác ở đây (ví dụ: Jump, Crouch...)
    public PlayerBaseState Jump()
    {
        return new PlayerJumpState(_context, this);
    }
    public PlayerBaseState Dodge()
    {
        return new PlayerDodgeState(_context, this);
    }
    public PlayerBaseState Grounded()
    {
        return new PlayerGroundedState(_context, this);
    }
    public PlayerBaseState Airborne()
    {
        return new PlayerAirborneState(_context, this);
    }
    public PlayerBaseState GetAttackStateForWeapon(int weaponId)
    {
        // Kiểm tra xem ID vũ khí có trong từ điển không
        if (attackStates.ContainsKey(weaponId))
        {
            return attackStates[weaponId].Invoke();
        }
        // Nếu không có, trả về trạng thái tấn công mặc định hoặc không vũ trang
        return attackStates[0].Invoke();
    }
    public PlayerBaseState Skill(SkillData skill)
    {
        return new PlayerSkillState(_context, this, skill);
    }
}