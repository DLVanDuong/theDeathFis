using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    public Animator animator;
    public readonly int HarizontalHash = Animator.StringToHash("Horizontal");
    public readonly int VerticalHash = Animator.StringToHash("Vertical");
    public readonly int IsSprintingHash = Animator.StringToHash("isSprinting");
    public readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    public readonly int IsEquippedHash = Animator.StringToHash("isEquipped");
    public readonly int AttackHash = Animator.StringToHash("Attack");
    public readonly int WeaponTypeHash = Animator.StringToHash("WeaponType");
    public readonly int JumpHash = Animator.StringToHash("Jump");
    public readonly int DodgeBlendTreeHash = Animator.StringToHash("Dodge");
    public readonly int CastSkillHash = Animator.StringToHash("CastSkill");
    public readonly int SkillIndexHash = Animator.StringToHash("SkillIndex");
    public readonly int TeleportHash = Animator.StringToHash("Teleport");

    public Vector3 DeltaPosition => animator.deltaPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();    
    }

    public void UpdateMovementParameters(float horizontal, float vertical, bool isSprinting)
    {
        animator.SetFloat(HarizontalHash, horizontal);
        animator.SetFloat(VerticalHash, vertical); 
        animator.SetBool(IsSprintingHash, isSprinting);
    }

    public void PlayTrigger(int hash)
    {
        animator.SetTrigger(hash);
    }

    // bật lại 1 trigger
    public void ResetTrigger(int hash)
    {
        animator.ResetTrigger(hash);
    }

    // kiểu bool
    public void SetBool(int hash, bool value)
    {
        animator.SetBool(hash, value);
    }
    // kiểu integer
    public void SetFloat(int hash, float value)
    {
        animator.SetFloat(hash, value);
    }
        // chuyển tiếp mượt sang trạng thái mới
        public void CrossFade(int hash, float transitionDuration)
    {
        animator.CrossFade(hash, transitionDuration);
    }
    // chuyển tiếp mượt sang trạng thái mới theo thời gian cố định
    public void CrossFadeInFixedTime(int hash, float duration)
    {
        animator.CrossFadeInFixedTime(hash, duration);
    }
    public void PlaySkill(int index)
    {        
        animator.SetFloat(SkillIndexHash, (float)index);
        animator.SetTrigger(CastSkillHash);
    }
}
