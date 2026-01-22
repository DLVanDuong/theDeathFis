// PlayerBaseState.cs
public abstract class PlayerBaseState
{
    protected PlayerStateMachine Ctx;       // Context - Tham chiếu đến State Machine chính (PlayerStateMachine.cs)
    protected PlayerStateFactory Factory;   // Tham chiếu đến nhà máy sản xuất state

    // Constructor để gán Context và Factory khi một state mới được tạo ra
    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        Ctx = currentContext;
        Factory = playerStateFactory;
    }

    // Các phương thức trừu tượng mà mọi state con BẮT BUỘC phải định nghĩa
    public abstract void EnterState();      // Logic khi vừa vào state
    public abstract void UpdateState();     // Logic cập nhật mỗi frame
    public abstract void ExitState();       // Logic khi thoát khỏi state
    public abstract void CheckSwitchStates(); // Logic kiểm tra để chuyển sang state khác

    // Phương thức dùng để chuyển đổi state
    protected void SwitchState(PlayerBaseState newState)
    {
        // Chạy logic thoát của state hiện tại
        ExitState();

        // Chạy logic vào của state mới
        newState.EnterState();

        // Cập nhật state hiện tại trong Context
        Ctx.CurrentState = newState;
    }
}