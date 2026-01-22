using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

[RequireComponent(typeof(EnemyStateMachine))]
public class BossController : MonoBehaviour
{
    private EnemyStateMachine stateMachine;
    private EnemyHealth enemyHealth;
    private NavMeshAgent agent;

    public BossData bossData;
    private int cachedMaxHP;

    [Header("Boss Abilities")]
    public BossAbility[] abilities;
    private int unlockedAbilityCount = 1;

    [Header("Boss Phases")]
    public BossPhase[] phases;
    private int currentPhaseIndex = 0;

    [Header("Cast Settings")]
    public Vector2 castIntervalRange = new Vector2(2.5f, 4f);

    [Header("Approach (vừa tầm)")]
    public float approachTolerance = 0.35f;
    public float maxApproachDuration = 3.5f;
    public float fallbackMoveSpeed = 4f;

    [Header("Smart Selector")]
    public float meleeWeight = 1.2f;
    public float rangedWeight = 1.0f;
    public float aoeWeight = 1.1f;
    public float dashWeight = 0.8f;
    public float summonWeight = 0.6f;
    public float stunWeight = 0.9f;

    public float inRangeBonus = 3f;
    public float distancePenaltyScale = 1f;
    public float sameAbilityPenalty = 2f;
    public bool preventImmediateRepeat = true;

    [Header("Line of Sight")]
    public bool requireLineOfSightForRanged = true;
    public LayerMask obstacleMask;

    public float phaseCastIntervalMultiplier = 0.9f;

    private float nextCastAt;
    private bool isCasting = false;
    private Coroutine approachRoutine;
    private BossAbility lastAbilityUsed;

    private void Awake()
    {
        stateMachine = GetComponent<EnemyStateMachine>();
        enemyHealth = GetComponent<EnemyHealth>();
        TryGetComponent(out agent);
    }

    private void Start()
    {
        // Lấy MaxHP thực tế sau khi spawner đã ApplyLevelScaling
        cachedMaxHP = Mathf.Max(1, Mathf.RoundToInt(stateMachine.GetMaxHealth()));
        ScheduleNextCast();
    }

    private void Update()
    {
        HandlePhaseTransition();

        if (!isCasting && Time.time >= nextCastAt && stateMachine.currentAbilityState == null && stateMachine.player)
        {
            UseSmartAbility();
        }
    }

    private void ScheduleNextCast()
    {
        nextCastAt = Time.time + Random.Range(castIntervalRange.x, castIntervalRange.y);
    }

    private float TypeWeight(AbilityType t)
    {
        switch (t)
        {
            case AbilityType.MeleeCombo: return meleeWeight;
            case AbilityType.RangedAttack: return rangedWeight;
            case AbilityType.AOE: return aoeWeight;
            case AbilityType.Dash: return dashWeight;
            case AbilityType.SummonMinions: return summonWeight;
            case AbilityType.Stun: return stunWeight;
            default: return 1f;
        }
    }

    private bool IsInRange(BossAbility ab)
    {
        if (stateMachine.player == null || ab == null) return false;
        float dist = Vector3.Distance(transform.position, stateMachine.player.position);
        return dist <= ab.range + approachTolerance;
    }

    private bool HasLineOfSight(BossAbility ab)
    {
        if (!requireLineOfSightForRanged || ab.type != AbilityType.RangedAttack) return true;
        if (!stateMachine.player) return false;

        Transform p = ab.spawnPoint ? ab.spawnPoint : transform;
        Vector3 origin = p.position + Vector3.up * 0.5f;
        Vector3 target = stateMachine.player.position + Vector3.up * 1.0f;
        Vector3 dir = (target - origin);
        float dist = dir.magnitude;
        if (dist <= 0.05f) return true;

        return !Physics.Raycast(origin, dir.normalized, dist, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private BossAbility SelectBestAbility(List<BossAbility> ready, float dist)
    {
        BossAbility best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var ab in ready)
        {
            float score = TypeWeight(ab.type);
            float delta = Mathf.Abs(dist - ab.range);
            score -= delta * distancePenaltyScale;
            if (IsInRange(ab)) score += inRangeBonus;
            if (!HasLineOfSight(ab)) score -= 1000f;
            if (preventImmediateRepeat && lastAbilityUsed == ab) score -= sameAbilityPenalty;

            if (score > bestScore)
            {
                bestScore = score;
                best = ab;
            }
        }
        return best;
    }

    public void UseSmartAbility()
    {
        if (isCasting) return;
        if (abilities == null || abilities.Length == 0) return;

        List<BossAbility> ready = new List<BossAbility>();
        int max = Mathf.Min(unlockedAbilityCount, abilities.Length);
        for (int i = 0; i < max; i++)
        {
            var ab = abilities[i];
            if (Time.time >= ab.lastCastTime + ab.cooldown)
                ready.Add(ab);
        }

        if (ready.Count == 0) { ScheduleNextCast(); return; }

        float dist = Vector3.Distance(transform.position, stateMachine.player.position);
        var chosen = SelectBestAbility(ready, dist);
        if (chosen == null) { ScheduleNextCast(); return; }

        if (!IsInRange(chosen) || !HasLineOfSight(chosen))
        {
            if (approachRoutine != null) StopCoroutine(approachRoutine);
            approachRoutine = StartCoroutine(ApproachThenCast(chosen));
        }
        else
        {
            chosen.lastCastTime = Time.time;
            isCasting = true;
            stateMachine.ChangeState(new BossAbilityState(stateMachine, chosen));
        }
    }

    private IEnumerator ApproachThenCast(BossAbility ab)
    {
        isCasting = true;

        float deadline = Time.time + maxApproachDuration;

        if (agent)
        {
            agent.isStopped = false;
            agent.stoppingDistance = Mathf.Max(ab.range - approachTolerance, 0.05f);
        }

        while (Time.time <= deadline && stateMachine.player)
        {
            if (IsInRange(ab) && HasLineOfSight(ab)) break;

            Vector3 targetPoint = DesiredPointForRange(ab.range);

            if (agent) agent.SetDestination(targetPoint);
            else transform.position = Vector3.MoveTowards(transform.position, targetPoint, fallbackMoveSpeed * Time.deltaTime);

            yield return null;
        }

        if (agent) agent.isStopped = true;

        if (!stateMachine.player || !IsInRange(ab) || !HasLineOfSight(ab))
        {
            isCasting = false;
            ScheduleNextCast();
            yield break;
        }

        Vector3 toPlayer = stateMachine.player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(toPlayer);

        ab.lastCastTime = Time.time;
        stateMachine.ChangeState(new BossAbilityState(stateMachine, ab));
    }

    private Vector3 DesiredPointForRange(float range)
    {
        Vector3 playerPos = stateMachine.player.position;
        Vector3 dir = playerPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
        dir.Normalize();

        float wantDist = Mathf.Max(range - approachTolerance, 0f);
        return playerPos - dir * wantDist;
    }

    private void HandlePhaseTransition()
    {
        if (enemyHealth == null || stateMachine.enemyData == null) return;
        if (cachedMaxHP <= 0) cachedMaxHP = (int)stateMachine.GetMaxHealth(); // fallback

        float hpPercent = (stateMachine.currentHealth / (float)cachedMaxHP) * 100f;

        if (currentPhaseIndex < phases.Length && hpPercent <= phases[currentPhaseIndex].healthThreshold)
        {
            EnterNewPhase(phases[currentPhaseIndex]);
            currentPhaseIndex++;
        }
    }

    private void EnterNewPhase(BossPhase newPhase)
    {
        stateMachine.OverrideDamage((int)newPhase.newDamage);
        if (unlockedAbilityCount < abilities.Length) unlockedAbilityCount++;
        castIntervalRange *= phaseCastIntervalMultiplier;
        castIntervalRange *= phaseCastIntervalMultiplier; // tăng nhịp độ
        if (agent) agent.acceleration *= 1.05f;
    }

    public void OnSkill()
    {
        var ab = stateMachine.currentAbility;
        if (ab == null) return;

        int finalDmg = ab.GetFinalDamage(stateMachine);

        switch (ab.type)
        {
            case AbilityType.MeleeCombo:
                if (ab.vfxPrefab)
                {
                    Transform p0 = ab.spawnPoint ? ab.spawnPoint : transform;
                    Instantiate(ab.vfxPrefab, p0.position, p0.rotation);
                }
                GetComponent<BossHitboxManager>()?.EnableAll();
                break;

            case AbilityType.Dash:
                if (ab.vfxPrefab)
                {
                    Transform p1 = ab.spawnPoint ? ab.spawnPoint : transform;
                    Instantiate(ab.vfxPrefab, p1.position, p1.rotation);
                }
                GetComponent<BossHitboxManager>()?.EnableAll();
                break;

            case AbilityType.AOE:
                if (ab.vfxPrefab)
                {
                    Transform p2 = ab.spawnPoint ? ab.spawnPoint : transform;
                    GameObject aoe = Instantiate(ab.vfxPrefab, p2.position, p2.rotation);
                    if (aoe.TryGetComponent<AOEHitbox>(out var aoeHit)) aoeHit.damage = finalDmg;
                }
                break;

            case AbilityType.RangedAttack:
                if (ab.vfxPrefab)
                {
                    Transform p3 = ab.spawnPoint ? ab.spawnPoint : transform;
                    GameObject projectileGO = Instantiate(ab.vfxPrefab, p3.position, p3.rotation);

                    if (projectileGO.TryGetComponent<Projectile>(out var projectile))
                        projectile.damage = finalDmg;

                    if (projectileGO.TryGetComponent<Rigidbody>(out var rb))
                    {
                        if (rb.isKinematic) rb.isKinematic = false;
                        rb.constraints = RigidbodyConstraints.FreezeRotation;
                        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                        rb.interpolation = RigidbodyInterpolation.Interpolate;
                        Vector3 dir = p3.forward;
                        rb.linearVelocity = dir * 10f;
                    }
                }
                break;

            case AbilityType.SummonMinions:
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.summonEnemy, 1f);
                if (ab.vfxPrefab)
                {
                    Transform p4 = ab.spawnPoint ? ab.spawnPoint : transform;
                    Instantiate(ab.vfxPrefab, p4.position, p4.rotation);
                }
                break;

            case AbilityType.Stun:
                if (ab.vfxPrefab)
                {
                    Transform p5 = ab.spawnPoint ? ab.spawnPoint : transform;
                    Instantiate(ab.vfxPrefab, p5.position, p5.rotation);
                }
                break;
        }
    }

    public void EndSkill()
    {
        GetComponent<BossHitboxManager>()?.DisableAll();
        lastAbilityUsed = stateMachine.currentAbility;

        if (stateMachine.currentAbilityState != null)
        {
            stateMachine.currentAbilityState.MarkSkillFinished();
            stateMachine.currentAbilityState = null;
        }
        isCasting = false;
        ScheduleNextCast();
    }
}
