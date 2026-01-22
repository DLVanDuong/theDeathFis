using UnityEngine;

public class BossAbilityState : IState
{
    private EnemyStateMachine stateMachine;
    private BossAbility ability;
    private bool abilityFinished;

    public BossAbilityState(EnemyStateMachine enemy, BossAbility ability)
    {
        stateMachine = enemy;
        this.ability = ability;
    }

    public void Enter()
    {
        Debug.Log($"[BossAbilityState] Bắt đầu skill: {ability.abilityName}");

        // Ngưng di chuyển khi thi triển skill
        stateMachine.agent.isStopped = true;

        // Gửi param cho BlendTree để chọn animation
        stateMachine.animator.SetFloat("AbilityType", ability.abilityIndex);
        stateMachine.animator.SetTrigger("UseAbility");

        // lưu lại skill hiện tại
        stateMachine.currentAbility = ability;
        stateMachine.currentAbilityState = this;
    }

    public void Execute()
    {
        if (abilityFinished)
            stateMachine.ChangeState(stateMachine._states.Chase());
    }

    public void Exit()
    {
        Debug.Log($"[BossAbilityState] Kết thúc skill: {ability.abilityName}");

        // Cho phép boss di chuyển lại
        stateMachine.agent.isStopped = false;
        stateMachine.currentAbility = null;
        stateMachine.currentAbilityState = null;
    }

    public void PerformAbilityEffect()
    {
        switch (ability.type)
        {
            case AbilityType.MeleeCombo:
                stateMachine.GetComponent<BossHitboxManager>()?.EnableAll();
                Debug.Log("Boss tung combo cận chiến!");
                break;

            case AbilityType.Dash:
                if (stateMachine.player != null)
                {
                    Vector3 dir = (stateMachine.player.position - stateMachine.transform.position).normalized;
                    stateMachine.agent.Move(dir * 5f);
                    Debug.Log("Boss dash tới player!");
                }
                break;

            case AbilityType.AOE:
                if (ability.vfxPrefab != null)
                {
                    Transform spawn = ability.spawnPoint != null ? ability.spawnPoint : stateMachine.transform;
                    GameObject.Instantiate(ability.vfxPrefab, spawn.position, Quaternion.identity);
                    Debug.Log("Boss tung skill AOE!");
                }
                break;

            case AbilityType.RangedAttack:
                if (ability.vfxPrefab != null && stateMachine.player != null)
                {
                    Transform spawn = ability.spawnPoint != null ? ability.spawnPoint : stateMachine.transform;
                    GameObject proj = GameObject.Instantiate(
                        ability.vfxPrefab,
                        spawn.position,
                        Quaternion.LookRotation(stateMachine.player.position - spawn.position)
                    );

                    if (proj.TryGetComponent<Projectile>(out var projectile))
                    {
                        // ✅ damage theo zone + bonusDamage
                        projectile.damage = ability.GetFinalDamage(stateMachine);
                    }

                    if (proj.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Vector3 dir = (stateMachine.player.position - spawn.position).normalized;
                        rb.linearVelocity = dir * 15f;
                    }
                    Debug.Log($"Boss bắn đạn tầm xa! Damage = {ability.GetFinalDamage(stateMachine)}");
                }
                break;

            case AbilityType.SummonMinions:
                Debug.Log("Boss triệu hồi minions!");
                break;
        }
    }
    public void MarkSkillFinished()
    {
        abilityFinished = true;
        stateMachine.GetComponent<BossHitboxManager>()?.DisableAll();
    }
}
