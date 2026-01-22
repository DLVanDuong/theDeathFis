using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

[DisallowMultipleComponent]
public class WeaponPickup : MonoBehaviour
{
    private WeaponInstance weaponInstance;

    [Header("Tên vật phẩm rớt")]
    [SerializeField] private TextMeshProUGUI worldNameText;

    private bool playerInRange = false;
    private PlayerInput playerInput;

    [Header("UI gợi ý")]
    [SerializeField] private GameObject pickupHintPrefab;
    private GameObject hintInstance;

    [Header("Pickup Settings")]
    public bool isDroppedFromEnemy = false;

    [Header("Tự biến mất sau")]
    [Tooltip("Thời gian tồn tại trên mặt đất (giây)")]
    public float lifetime = 30f;
    private float spawnTime;

    [Tooltip("Khoảng thời gian trước khi biến mất để bắt đầu chớp nháy (giây)")]
    public float blinkDuration = 3f;

    private bool isBlinking = false;
    private Renderer[] renderers;

    // =========================
    // AUTO BIND TMP (KHỎI KÉO)
    // =========================
    private void AutoBindWorldNameText()
    {
        if (worldNameText != null) return;

        // Tự tìm TMP trong con (kể cả object đang tắt)
        worldNameText = GetComponentInChildren<TextMeshProUGUI>(true);

        // Nếu bạn lỡ dùng TextMeshPro (3D) thay vì UGUI thì dòng này sẽ không cần,
        // nhưng để an toàn thì vẫn giữ.
        if (worldNameText == null)
        {
            var tmp3D = GetComponentInChildren<TextMeshPro>(true);
            // Không convert được TextMeshPro -> TextMeshProUGUI, nên chỉ cảnh báo.
            if (tmp3D != null)
            {
                Debug.LogWarning("[WeaponPickup] Bạn đang dùng TextMeshPro (3D). Hãy dùng TextMeshProUGUI trong Canvas World Space.");
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoBindWorldNameText();
    }
#endif

    private void Awake()
    {
        AutoBindWorldNameText();

        spawnTime = Time.time;
        renderers = GetComponentsInChildren<Renderer>(true);

        var col = GetComponent<Collider>();

        // ✅ Nếu đang gắn trong EquipmentManager (đang trang bị) -> tắt tên + vô hiệu
        if (GetComponentInParent<EquipmentManager>() != null)
        {
            if (worldNameText != null)
                worldNameText.gameObject.SetActive(false);

            if (col) col.enabled = false;

            enabled = false;
            return;
        }

        // ✅ Nếu KHÔNG phải đồ rơi -> tắt tên + vô hiệu
        if (!isDroppedFromEnemy)
        {
            if (worldNameText != null)
                worldNameText.gameObject.SetActive(false);

            if (col) col.enabled = false;

            enabled = false;
            return;
        }

        // ✅ Đồ rơi thật -> bật collider + bật tên
        if (worldNameText != null)
            worldNameText.gameObject.SetActive(true);

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }
    }
    private void Update()
    {
        if (!isDroppedFromEnemy) return;

        float elapsed = Time.time - spawnTime;
        float remaining = lifetime - elapsed;

        // Bắt đầu chớp nháy 3s cuối
        if (!isBlinking && remaining <= blinkDuration && remaining > 0)
        {
            StartCoroutine(BlinkEffect());
            isBlinking = true;
        }

        // Hết thời gian thì biến mất
        if (elapsed >= lifetime)
        {
            if (hintInstance) Destroy(hintInstance);
            StartCoroutine(FadeAndDestroy());
        }
    }

    private IEnumerator BlinkEffect()
    {
        float blinkSpeed = 0.2f;
        while (Time.time - spawnTime < lifetime)
        {
            SetRenderersVisible(false);
            yield return new WaitForSeconds(blinkSpeed);
            SetRenderersVisible(true);
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        float fadeTime = 0.5f;
        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / fadeTime);

            foreach (var rend in renderers)
            {
                if (rend != null && rend.material != null && rend.material.HasProperty("_Color"))
                {
                    Color c = rend.material.color;
                    c.a = alpha;
                    rend.material.color = c;
                }
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !isDroppedFromEnemy) return;

        playerInRange = true;
        playerInput = other.GetComponent<PlayerInput>();
        if (playerInput != null)
            playerInput.actions["Interact"].started += OnInteract;

        if (pickupHintPrefab != null && hintInstance == null)
        {
            hintInstance = Instantiate(pickupHintPrefab, transform);
            hintInstance.transform.localPosition = Vector3.up * 1.2f;
            hintInstance.transform.localRotation = Quaternion.identity;

            var text = hintInstance.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null) text.text = "Nhấn F để nhặt";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (playerInput != null)
        {
            playerInput.actions["Interact"].started -= OnInteract;
            playerInput = null;
        }

        if (hintInstance != null)
        {
            Destroy(hintInstance);
            hintInstance = null;
        }
    }

    private void OnDestroy()
    {
        if (playerInput != null)
            playerInput.actions["Interact"].started -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!playerInRange || weaponInstance == null) return;

        Inventory.Instance?.AddWeapon(weaponInstance);

        if (hintInstance) Destroy(hintInstance);

        if (playerInput != null)
            playerInput.actions["Interact"].started -= OnInteract;

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.pickup, 1f);

        Destroy(gameObject);
    }

    public void SetWeaponInstance(WeaponInstance inst)
    {
        weaponInstance = inst;

        AutoBindWorldNameText();

        // nếu đang trang bị -> không hiện text
        if (GetComponentInParent<EquipmentManager>() != null)
        {
            if (worldNameText != null)
                worldNameText.gameObject.SetActive(false);
            return;
        }

        if (worldNameText == null || weaponInstance == null || weaponInstance.template == null)
            return;

        // ✅ đảm bảo đồ rơi sẽ bật text lên
        worldNameText.gameObject.SetActive(true);

        string baseName = weaponInstance.template.weaponName;
        string plus = weaponInstance.upgradeLevel > 0 ? $" +{weaponInstance.upgradeLevel}" : "";
        string rarityName = RarityDisplay.GetRarityName(weaponInstance.rarity);

        worldNameText.text = $"{baseName}{plus} [{rarityName}]";
        worldNameText.color = RarityDisplay.GetRarityColor(weaponInstance.rarity);
    }

}
