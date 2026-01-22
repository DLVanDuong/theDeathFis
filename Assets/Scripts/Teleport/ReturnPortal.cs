using System.Collections;
using UnityEngine;

public class ReturnPortal : MonoBehaviour
{
    // Vị trí player sẽ quay về (được set từ ReturnPortalManager)
    private Vector3 returnPosition;

    [Header("Teleport Settings")]
    public float teleportDelay = 1.5f; // Thời gian chờ animation trước khi teleport

    [Header("VFX (có thì gán, không cũng được)")]
    public GameObject teleportInEffect;   // VFX lúc xuất hiện ở chỗ mới
    public GameObject teleportOutEffect;  // VFX lúc biến mất ở chỗ cũ

    private bool hasTeleported = false;
    private Collider triggerCol;

    void Awake()
    {
        triggerCol = GetComponent<Collider>();
    }

    // Gọi từ ReturnPortalManager khi tạo cổng
    public void SetReturnPosition(Vector3 pos)
    {
        returnPosition = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTeleported) return;                // tránh teleport 2 lần
        if (!other.CompareTag("Player")) return;  // chỉ nhận player

        StartCoroutine(TeleportSequence(other.transform.root.gameObject));
    }

    private IEnumerator TeleportSequence(GameObject player)
    {
        hasTeleported = true;
        if (triggerCol != null) triggerCol.enabled = false;

        Transform playerRoot = player.transform.root;

        // Tắt CharacterController nếu có
        var cc = playerRoot.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 1. Play animation Teleport (nếu dùng AnimatorManager)
        var animMgr = playerRoot.GetComponent<AnimatorManager>();
        if (animMgr != null)
        {
            animMgr.PlayTrigger(animMgr.TeleportHash);
        }
        else
        {
            Animator anim = playerRoot.GetComponentInChildren<Animator>();
            if (anim != null)
                anim.SetTrigger("Teleport");
        }

        // 2. VFX biến mất ở Làng
        if (teleportOutEffect != null)
        {
            GameObject fxOut = Instantiate(teleportOutEffect, playerRoot.position, Quaternion.identity);
            Destroy(fxOut, 3f);   // tự xoá sau 3 giây
        }

        // 3. Đợi animation chạy
        yield return new WaitForSeconds(teleportDelay);

        // 4. Teleport tới vị trí đã lưu
        Vector3 oldPos = playerRoot.position;
        playerRoot.position = returnPosition;

        // 5. VFX xuất hiện ở chỗ mới
        if (teleportInEffect != null)
        {
            GameObject fxIn = Instantiate(teleportInEffect, returnPosition, Quaternion.identity);
            Destroy(fxIn, 3f);
        }

        // 6. Bật lại CharacterController
        if (cc != null) cc.enabled = true;

        Debug.Log($"[ReturnPortal] Teleport player từ {oldPos} tới {returnPosition}");

        // 7. Dùng xong thì xoá luôn cổng
        Destroy(gameObject, 0.1f);
    }
}
