using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReturnPortalManager : MonoBehaviour
{
    [Header("Prefab cổng (có Portal_Controller + ReturnPortal)")]
    public GameObject portalPrefab;

    [Header("Vị trí spawn CỔNG ở Làng")]
    public Transform townPortalSpawnPoint;

    [Header("Vị trí PLAYER xuất hiện ở Làng (không trùng cổng)")]
    public Transform townPlayerSpawnPoint;

    [Header("VFX Recall (tuỳ chọn)")]
    public GameObject recallOutVFX;   // hiệu ứng lúc BẮT ĐẦU biến về (nơi hiện tại)
    public GameObject recallInVFX;    // hiệu ứng lúc VỪA TỚI làng

    [Header("Thời gian tụ phép trước khi biến về")]
    public float recallDelay = 2f;    // bạn muốn ~2s

    private Vector3 savedReturnPosition;   // chỗ sẽ quay lại khi đi vào cổng
    private GameObject spawnedPortal;
    private PlayerControls inputActions;

    void Awake()
    {
        inputActions = new PlayerControls();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Return.performed += OnReturnPerformed;
    }

    void OnDisable()
    {
        inputActions.Player.Return.performed -= OnReturnPerformed;
        inputActions.Player.Disable();
    }

    private void OnReturnPerformed(InputAction.CallbackContext ctx)
    {
        StartCoroutine(RecallCoroutine());
    }

    private IEnumerator RecallCoroutine()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || portalPrefab == null ||
            townPortalSpawnPoint == null || townPlayerSpawnPoint == null)
        {
            Debug.LogWarning("[ReturnPortalManager] Thiếu tham chiếu!");
            yield break;
        }

        Transform root = player.transform.root;

        // 1. Lưu vị trí hiện tại để cổng dùng quay lại
        savedReturnPosition = root.position;

        // 2. VFX tụ phép tại chỗ cũ
        if (recallOutVFX != null)
        {
            GameObject fxOut = Instantiate(recallOutVFX, root.position, Quaternion.identity);
            Destroy(fxOut, 3f);
        }

        // (Nếu muốn animation recall thì bật trigger ở đây – bỏ qua cho đỡ rối)
        // var animMgr = root.GetComponent<AnimatorManager>();
        // if (animMgr != null) animMgr.PlayTrigger(animMgr.TeleportHash);

        // 3. Chờ 2s (recallDelay) rồi mới biến về
        yield return new WaitForSeconds(recallDelay);

        // 4. Spawn / làm mới cổng ở Làng
        if (spawnedPortal != null)
            Destroy(spawnedPortal);

        spawnedPortal = Instantiate(
            portalPrefab,
            townPortalSpawnPoint.position,
            townPortalSpawnPoint.rotation
        );

        // Gửi vị trí quay lại cho script ReturnPortal trên cổng
        var portal = spawnedPortal.GetComponent<ReturnPortal>();
        if (portal != null)
            portal.SetReturnPosition(savedReturnPosition);

        // 5. Teleport Player tới Làng (KHÔNG phải chỗ cổng, mà là TownPlayerSpawnPoint)
        var cc = root.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        root.position = townPlayerSpawnPoint.position;

        if (cc != null) cc.enabled = true;

        // 6. VFX xuất hiện ở Làng
        if (recallInVFX != null)
        {
            GameObject fxIn = Instantiate(recallInVFX, root.position, Quaternion.identity);
            Destroy(fxIn, 3f);
        }

        Debug.Log("[ReturnPortal] Player recall về Làng tại " + townPlayerSpawnPoint.position);
    }
}
