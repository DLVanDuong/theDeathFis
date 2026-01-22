// PlayerSkillState.cs
using UnityEngine;

public class PlayerSkillState : PlayerBaseState
{
    private SkillData Skill;
    
    public PlayerSkillState(PlayerStateMachine ctx, PlayerStateFactory factory, SkillData skill)
        : base(ctx, factory)
    {
        this.Skill = skill;
    }

    public override void EnterState()
    {     
        Ctx.animatorManager.PlaySkill(Skill.blendTreeIndex);
        Ctx.skillManager.currentSkill = Skill;
        Ctx.StartCoroutine(CoSkillDuration());
    }

    public override void UpdateState() { }

    public override void ExitState()
    {
        Ctx.characterController.enabled = true;
        Ctx.skillManager.currentSkill = null;
    }

    public override void CheckSwitchStates()
    {
        // Điều kiện: Chỉ chuyển trạng thái khi animation skill đã kết thúc
       
    }

    // Bổ sung: Phương thức này sẽ được gọi từ Animation Event
    private System.Collections.IEnumerator CoSkillDuration()
    {
        // Đợi trong một khoảng thời gian bằng với cooldown của skill
        // Vì anim và cooldown là hai biến riêng, bạn có thể thêm một biến 'skillDuration' vào SkillData nếu cần
        yield return new WaitForSeconds(Skill.cooldown);

        // Sau khi hết thời gian, chuyển về trạng thái Idle
        SwitchState(Factory.Idle());
    }
}