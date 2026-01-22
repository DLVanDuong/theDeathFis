// EnemyStateMachine.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyStateMachine : MonoBehaviour
{
    // === Trạng thái FSM ===
    [HideInInspector] public IState currentState;
    public bool isAttacking;

    [HideInInspector] public EnemyStateFactory _states;

    // == Tham Chiếu Component ==
    [Header("Componets")]
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Transform player;
    private PlayerStateMachine playerControler;
    [HideInInspector] public EnemyHitbox enemyHitbox;
    [SerializeField] private Transform projectileSpawnPoint;

    // == thông số AI ==
    [Header("AI Parameters")]
    [HideInInspector] public int currentPoint = 0;
    public float walkPointRange = 25f;
    [HideInInspector] public Vector3 randomPatrolTarget;

    // == Trọng lực cho Enemy ==
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    private Vector3 verticalVelocity;

    public int Level { get; private set; } = 1;

    [Header("Level Scaling")]
    [SerializeField] private float healthGrowthPerLevel = 0.05f;  // 5% mỗi level
    [SerializeField] private float damageGrowthPerLevel = 0.02f;  // 2% mỗi level

    private int baseHealth;
    private int baseDamage;

    private int scaledHealth;
    private int scaledDamage;

    [Header("Enemy Data")]
    public EnemyData enemyData;
    [HideInInspector] public int currentHealth;
    private bool isDead = false;

    [Header("Health Bar UI")]
    [SerializeField] private GameObject healthBarUI;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private float healthBarAutoHideDelay = 3f;
    [SerializeField] private TMP_Text hpText;
    private Coroutine hideBarCo;

    [HideInInspector] public float lastAttackTime;
    public BossAbility currentAbility;
    public BossAbilityState currentAbilityState;

    [HideInInspector] public ZoneArea myZone;
    [HideInInspector] public bool hasLeveled;
    [HideInInspector] public int enemyLevel = 1;
    private void Awake()
    {
        _states = new EnemyStateFactory(this);
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerControler = playerObject.GetComponent<PlayerStateMachine>();
        }

        enemyHitbox = GetComponentInChildren<EnemyHitbox>();
        if (enemyHitbox != null)
        {
            enemyHitbox.enemyStateMachine = this;
        }

        if (enemyData != null)
        {
            // Lưu lại giá trị gốc từ ScriptableObject (không bị thay đổi khi scale)
            baseHealth = enemyData.health;
            baseDamage = enemyData.damage;

            scaledHealth = baseHealth;
            scaledDamage = baseDamage;

            currentHealth = baseHealth;
            lastAttackTime = -enemyData.attackCooldown;
        }
    }
    void Start()
    {
        ChangeState(_states.Idle());
        if (healthBarUI != null)
        {
            UpdateHealthUI();
            healthBarUI.SetActive(false);
        }
    }
    void Update()
    {
        currentState?.Execute();

        // Cập nhật Animator nếu agent đang di chuyển hoặc đã dừng
        UpDateMovementAnimator();

        // Xử lý trọng lực chỉ khi NavMeshAgent không hoạt động
        if (!agent.enabled || agent.isStopped)
        {
            HandleGravity();
        }
    }
    public void OverrideDamage(int newDmg)
    {
        scaledDamage = newDmg;
        
    }
    private void UpDateMovementAnimator()
    {
        float currentSpeed = agent.velocity.magnitude;
        if (enemyData != null && enemyData.animationData != null)
        {
            animator.SetFloat(enemyData.animationData.speedParam, currentSpeed);
        }
        else
        {
            animator.SetFloat("Speed", currentSpeed);
        }
    }
    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        

        // Hiện thanh máu và reset hẹn giờ 5s
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(true);

            // Nếu đang chạy coroutine ẩn trước đó thì dừng lại
            if (hideBarCo != null) StopCoroutine(hideBarCo);

            // Bắt đầu coroutine mới
            hideBarCo = StartCoroutine(HideBarLater());
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            isDead = true;
            ChangeState(_states.Dead());
            if (healthBarUI != null) healthBarUI.SetActive(false);
        }
    }
    private System.Collections.IEnumerator HideBarLater()
    {
      
        yield return new WaitForSeconds(healthBarAutoHideDelay);

        if (!isDead && healthBarUI != null)
            healthBarUI.SetActive(false);
    }
    private void UpdateHealthUI()
    {
        currentHealth = Mathf.Max(0, currentHealth);

        if (healthFillImage != null && scaledHealth > 0)
            healthFillImage.fillAmount = (float)currentHealth / scaledHealth;

        if (hpText != null)
            hpText.text = $"{currentHealth:n0}/{scaledHealth:n0}";
    }
    public void StartAttackAnimation()
    {
        isAttacking = true;
        if (enemyData != null && enemyData.animationData != null)
        {
            animator.SetTrigger(enemyData.animationData.attackTrigger);
        }
        else
        {
            animator.SetTrigger("Attack");
        }
        lastAttackTime = Time.time;
    }
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }
    public void ShootProjectile(bool useGravity)
    {
        RangedEnemyData rangedEnemyData = enemyData as RangedEnemyData;
        if (rangedEnemyData == null)
        {
            
            return;
        }
        if (rangedEnemyData.projectilePrefab == null)
        {
            
            return;
        }
        if (projectileSpawnPoint == null)
        {
           
            return;
        }

        Vector3 direction = (player.position - projectileSpawnPoint.position).normalized;
        direction.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        rotation *= Quaternion.Euler(90f, 180f, 0f);
        GameObject projectileGO = Instantiate(rangedEnemyData.projectilePrefab,
            projectileSpawnPoint.position,
            rotation);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(scaledDamage, rangedEnemyData.projectileSpeed, gameObject.layer, direction, useGravity);
           
        }
        lastAttackTime = Time.time;
    }

    public void HitboxConnectedWithPlayer(Collider playerCollider)
    {
        HealthPlayer playerHealth = playerCollider.GetComponent<HealthPlayer>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(scaledDamage);
            
        }
    }
    public bool CanDetectPlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > enemyData.singhtRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > enemyData.singhtAngle / 2) return false;

        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(raycastOrigin, directionToPlayer, out hit, enemyData.singhtRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
    public void EnableEnemyHitbox()
    {
        if (enemyHitbox != null)
        {
            enemyHitbox.EnableHitbox();
        }
    }
    public void DisableEnemyHitbox()
    {
        if (enemyHitbox != null) enemyHitbox.DisableHitbox();
    }
    private void HandleGravity()
    {
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravity * Time.deltaTime;
        transform.position += verticalVelocity * Time.deltaTime;
    }
    public void SearchWalkPoint()
    {
        float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float ramdomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

        Vector3 randomPoint = new Vector3(transform.position.x + ramdomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPoint, out hit, walkPointRange, NavMesh.AllAreas))
        {
            randomPatrolTarget = hit.position;
        }
        else
        {
            randomPatrolTarget = transform.position;
        }
    }
    public void ApplyLevelScaling(int level)
    {
        if (hasLeveled || enemyData == null) return;

        enemyLevel = level;
        scaledHealth = Mathf.RoundToInt(baseHealth * (1f + healthGrowthPerLevel * (level - 1)));
        scaledDamage = Mathf.RoundToInt(baseDamage * (1f + damageGrowthPerLevel * (level - 1)));

        currentHealth = scaledHealth;
        hasLeveled = true;

        UpdateHealthUI(); // <- thêm dòng này
       
    }
    public int GetMaxHealth()
    {
        return scaledHealth;
    }

    public int GetDamage(int bonus = 0, float multiplier = 1f)
    {
        return Mathf.RoundToInt(scaledDamage * multiplier) + bonus;
    }
    public Vector3 ClampToZone(Vector3 target)
    {
        if (myZone == null) return target;
        Vector3 center = myZone.transform.position;
        Vector3 flat = new Vector3(target.x - center.x, 0f, target.z - center.z);
        Vector3 clamped = flat.magnitude <= myZone.radius
            ? target
            : center + flat.normalized * myZone.radius;

        clamped.y = target.y;
        return myZone.SampleOnNavMesh(clamped, 2f);
    }
    private void OnDrawGizmos()
    {
        // Vòng tròn tầm nhìn (Sight Range)
        Gizmos.color = Color.yellow;
        if (enemyData.singhtRange > 0)
        {
            Gizmos.DrawWireSphere(transform.position, enemyData.singhtRange);
        }

        // Tầm tấn công (Attack Range)
        Gizmos.color = Color.red;
        if (enemyData != null && enemyData.attackRange > 0)
        {
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
        }

        // Góc nhìn (Field of View - FOV)
        Gizmos.color = Color.cyan;
        if (enemyData.singhtRange > 0 && enemyData.singhtAngle > 0)
        {
            Vector3 forward = transform.forward;
            Vector3 origin = transform.position;

            Vector3 fovLineLeft = Quaternion.AngleAxis(-enemyData.singhtAngle / 2, transform.up) * forward * enemyData.singhtRange;
            Vector3 fovLineRight = Quaternion.AngleAxis(enemyData.singhtAngle / 2, transform.up) * forward * enemyData.singhtRange;

            Gizmos.DrawRay(origin, fovLineLeft);
            Gizmos.DrawRay(origin, fovLineRight);
            Gizmos.DrawLine(origin + fovLineLeft, origin + fovLineRight);
        }
    }   
}