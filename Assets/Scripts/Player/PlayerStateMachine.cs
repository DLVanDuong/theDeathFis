// PlayerStateMachine.cs (đã lược bỏ Save/Load cũ)
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AnimatorManager))]
[RequireComponent(typeof(EquipmentManager))]
public class PlayerStateMachine : MonoBehaviour
{
    private PlayerBaseState currentState;
    public PlayerStateFactory states;

    public CharacterController characterController;
    public AnimatorManager animatorManager { get; private set; }
    public EquipmentManager equipmentManager;
    private QuickSlotManager quickSlotManager;
    [HideInInspector] public SkillManager skillManager;
    public PlayerStatsRuntime playerStat { get; private set; }

    [Header("InputSetting")]
    private PlayerControls inputActions;
    private Vector2 currentMoveInput;
    public bool isSprinting = false;
    public bool isAttackPressed = false;
    public bool isJumpPressed = false;
    public bool isDodgePressed = false;

    [Header("Camera & Rotation")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] public float jumpForce = 5f;
    public float playerVelocityY;

    [Header("Projectile")]
    [SerializeField] public GameObject arrowPrefab;
    [SerializeField] public Transform arrowSpawnPoint;

    [Header("Testing")]
    [Tooltip("Kéo các trang bị bạn muốn nhân vật tự mặc khi bắt đầu game vào đây.")]
    [SerializeField] private EquipmentData[] testWeapon;
    private WeaponHitbox currentWeaponHitbox;

    // getters
    public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }
    public Vector2 CurrentMoveInput { get => currentMoveInput; }
    public bool IsAttackPressed { get => isAttackPressed; set => isAttackPressed = value; }
    public bool IsDodgePressed { get => isDodgePressed; set => isDodgePressed = value; }
    public float PlayerVelocityY { get => playerVelocityY; set => playerVelocityY = value; }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animatorManager = GetComponent<AnimatorManager>();
        equipmentManager = GetComponent<EquipmentManager>();
        quickSlotManager = GetComponent<QuickSlotManager>();
        skillManager = GetComponent<SkillManager>();
        states = new PlayerStateFactory(this);

        currentState = states.Idle();
        currentState.EnterState();

        inputActions = new PlayerControls();
        inputActions.Player.Move.performed += ctx => currentMoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => currentMoveInput = Vector2.zero;
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
        inputActions.Player.Attack.performed += ctx => isAttackPressed = true;
        inputActions.Player.Dodge.performed += ctx => isDodgePressed = true;
        inputActions.Player.UseItem1.performed += ctx => quickSlotManager?.UseItemInSlot(0);
        inputActions.Player.UseItem2.performed += ctx => quickSlotManager?.UseItemInSlot(1);

        if (cameraTransform == null) { cameraTransform = Camera.main != null ? Camera.main.transform : null; }

        // Chỉ auto-equip test vũ khí khi KHÔNG load game
        if (PlayerPrefs.GetInt("LoadGame", 0) == 0)
        {
            if (testWeapon != null && testWeapon.Length > 0)
            {
                equipmentManager.UnequipAll();
                foreach (EquipmentData item in testWeapon)
                {
                    if (item == null) continue;

                    if (item.slot == EquipmentSlot.RightHand && item is WeaponData swordData)
                    {
                        var sword = new WeaponInstance(swordData);
                        equipmentManager.EquipWeaponInstance(EquipmentSlot.RightHand, sword);
                        skillManager?.EquipWeapon(sword);
                    }
                    else if (item.slot == EquipmentSlot.LeftHand && item is WeaponData shieldData)
                    {
                        var shield = new WeaponInstance(shieldData);
                        equipmentManager.EquipWeaponInstance(EquipmentSlot.LeftHand, shield);
                    }
                    else
                    {
                        equipmentManager.Equip(item);
                    }
                }
            }
        }
    }

    void Start()
    {
        if (playerStat == null)
        {
            var playerLevelSystem = GetComponent<PlayerLevelSystem>();
            if (playerLevelSystem != null)
                playerStat = playerLevelSystem.playerStats;
        }
    }

    void OnEnable() { inputActions.Player.Enable(); }
    void OnDisable() { if (inputActions != null) inputActions.Disable(); }

    void Update()
    {
        HandleGravity();
        animatorManager.SetBool(animatorManager.IsGroundedHash, characterController.isGrounded);

        int weaponTypeID = equipmentManager.CurrentWeaponType();
        animatorManager.SetFloat(animatorManager.WeaponTypeHash, weaponTypeID);

        currentState.UpdateState();
    }

    void HandleGravity()
    {
        if (characterController.isGrounded && playerVelocityY < 0.0f) playerVelocityY = -2.0f;
        else playerVelocityY += gravity * Time.deltaTime;
        characterController.Move(new Vector3(0, playerVelocityY, 0) * Time.deltaTime);
    }

    public void HandleRotation()
    {
        if (cameraTransform == null) return;
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        if (cameraForward != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraForward), rotationSpeed * Time.deltaTime);
    }

    public void HandleAnimation()
    {
        animatorManager.UpdateMovementParameters(currentMoveInput.x, currentMoveInput.y, isSprinting);
    }

    private void OnAnimatorMove()
    {
        if (characterController != null)
            characterController.Move(animatorManager.DeltaPosition);
    }

    public void OnAttackAnimationEnd() { currentState.CheckSwitchStates(); }
    public void OnDodgeAnimationEnd() { currentState.CheckSwitchStates(); }

    public void EnableHitbox() { equipmentManager.EnableCurrentWeaponHitbox(); }
    public void DisableHitbox() { equipmentManager.DisableCurrentWeaponHitbox(); }

    public void PlayerFootSound()
    {
        var am = AudioManager.Instance;
        if (am != null) am.PlaySFX(isSprinting ? am.footstepRun : am.footstepWalk, 0.8f);
    }
    public void OnWeaponSwingSound() { AudioManager.Instance?.PlaySFX(AudioManager.Instance.swordSwing, 0.9f); }

    // Animation Event gọi khi tung skill
    public void OnSkillEvent() { skillManager?.SpawnSkillEffect(); }

    public void PerformAttack(WeaponData weapon = null, SkillData skill = null)
    {
        if (currentWeaponHitbox == null) return;
        var equipMgr = FindAnyObjectByType<EquipmentManager>();
        var bonus = equipMgr != null ? equipMgr.GetEquippedWeaponBonus() : default;
        int finalDamage = DamageCalculator.GetFinalDamage(playerStat, bonus, skill);
        currentWeaponHitbox.SetDamage(finalDamage);
        currentWeaponHitbox.EnableHitbox();
    }

    public void ShootArrow()
    {
        if (equipmentManager == null) return;

        int type = equipmentManager.CurrentWeaponType();
        if (type != 3) return;

        if (!equipmentManager.TryGetEquippedInstance(EquipmentSlot.RightHand, out WeaponInstance quiver) ||
            quiver == null || quiver.template.weaponTypeID != 8)
        {
            ArrowUIManager.Instance?.ShowArrowMessage("Bạn chưa trang bị túi cung!");
            return;
        }
        if (!equipmentManager.TryGetEquippedInstance(EquipmentSlot.LeftHand, out WeaponInstance bow) || bow == null)
        {
            ArrowUIManager.Instance?.ShowArrowMessage("Bạn chưa trang bị cung!");
            return;
        }
        if (bow.template.arrowType != quiver.template.arrowType)
        {
            ArrowUIManager.Instance?.ShowArrowMessage(
                $"Túi không khớp với loại cung đang dùng!\n" +
                $"Cung: {bow.template.weaponName}\n" +
                $"Túi đang đeo: {quiver.template.weaponName}\n" +
                $"Cần loại: Cùng Tên với Vũ Khí ");
            return;
        }
        if (quiver.template.arrowPrefabOverride == null)
        {
            ArrowUIManager.Instance?.ShowArrowMessage("Túi cung chưa có mũi tên!");
            return;
        }

        Transform spawn = equipmentManager.GetWeaponHoldPointL();
        if (spawn == null) spawn = transform;
        Quaternion rot = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(90, 0, 0);

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.bowShoot, 0.9f);

        GameObject arrow = Instantiate(quiver.template.arrowPrefabOverride, spawn.position, rot);
        if (arrow.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = transform.forward * 50f;

        if (arrow.TryGetComponent<Arrow>(out var arrowScript))
        {
            var bonus = equipmentManager.GetEquippedWeaponBonus();
            arrowScript.damage = DamageCalculator.GetFinalDamage(playerStat, bonus);
        }
    }

    public void CancelAllCombat()
    {
        equipmentManager?.DisableCurrentWeaponHitbox();
        isAttackPressed = false;
        currentState?.CheckSwitchStates();
    }

    public void OnPlayerDeath_HardStop() { CancelAllCombat(); }
}
