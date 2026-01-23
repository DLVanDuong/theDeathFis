using System.Collections.Generic;
using UnityEngine;

public class DynamicTextManager : MonoBehaviour
{
    // === Static refs (runtime) ===
    public static DynamicTextData defaultData;
    public static DynamicTextData playerDamageData;
    public static DynamicTextData enemyDamageData;
    public static GameObject canvasPrefab;   // prefab text (world-space)
    public static Transform mainCamera;      // dùng cho billboard

    // === Inspector (assign ở scene) ===
    [SerializeField] private DynamicTextData _defaultData;
    [SerializeField] private DynamicTextData _playerDamageData;
    [SerializeField] private DynamicTextData _enemyDamageData;
    [SerializeField] private GameObject _canvasPrefab;
    [SerializeField] private Transform _mainCamera;

    // ==== Anti-overlap lanes ====
    private static readonly Dictionary<Transform, int> _laneIndex = new Dictionary<Transform, int>();
    private static int _globalSorting = 0;

    private void Awake()
    {
        defaultData = _defaultData;
        playerDamageData = _playerDamageData;
        enemyDamageData = _enemyDamageData;
        canvasPrefab = _canvasPrefab;

        // Tự bắt camera nếu chưa gán
        mainCamera = _mainCamera ? _mainCamera
                                 : (Camera.main ? Camera.main.transform : null);

        if (!canvasPrefab)
            Debug.LogError("[DynamicTextManager] _canvasPrefab chưa gán!");
    }

    // --- 2D variant (nếu bạn có text 2D riêng) ---
    public static void CreateText2D(Vector2 position, string text, DynamicTextData data)
    {
        if (!canvasPrefab) { Debug.LogError("[DynamicTextManager] canvasPrefab null"); return; }
        GameObject go = Instantiate(canvasPrefab, position, Quaternion.identity);
        var comp = go.transform.GetComponent<DynamicText2D>();
        if (comp) comp.Initialise(text, data);
    }

    // --- 3D variant cơ bản (không xếp làn) ---
    public static void CreateText(Vector3 position, string text, DynamicTextData data)
    {
        if (!canvasPrefab) { Debug.LogError("[DynamicTextManager] canvasPrefab null"); return; }
        GameObject go = Instantiate(canvasPrefab, position, Quaternion.identity);
        var comp = go.transform.GetComponent<DynamicText>();
        if (comp) comp.Initialise(text, data);
    }

    // --- 3D variant chống đè: xếp "làn" + giật ngang nhẹ ---
    public static void CreateTextStacked(
        Transform anchor,                 // thường là enemy.transform
        string text,
        DynamicTextData data,
        float baseUpOffset = 1.2f,        // cao hơn đầu
        float verticalStep = 0.14f,      // mỗi hit cao hơn chút
        float horizontalSpread = 0.22f,   // lệch ngang nhẹ để tách
        Vector3 extraLocalOffset = default
    )
    {
        if (!canvasPrefab) { Debug.LogError("[DynamicTextManager] canvasPrefab null"); return; }

        if (!anchor) { CreateText(Vector3.zero, text, data); return; }

        if (!_laneIndex.TryGetValue(anchor, out int i)) i = 0;

        // Rải ngang theo "góc vàng" để ít trùng
        float a = i * 2.399963f; // ~137.5°
        Vector3 side = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * horizontalSpread;

        Vector3 worldPos =
            anchor.TransformPoint(extraLocalOffset) +
            Vector3.up * (baseUpOffset + i * verticalStep) +
            side;

        GameObject go = Instantiate(canvasPrefab, worldPos, Quaternion.identity);
        var comp = go.transform.GetComponent<DynamicText>();
        if (comp) comp.Initialise(text, data);

        // Ưu tiên vẽ text mới phía trên
        var canvas = go.GetComponentInChildren<Canvas>();
        if (canvas) { canvas.overrideSorting = true; canvas.sortingOrder = ++_globalSorting; }

        // Tăng làn và quay vòng
        i = (i + 6) % 6; // 6 làn; muốn nhiều hơn thì đổi số này
        _laneIndex[anchor] = i;
    }

    /// <summary>Gọi khi enemy chết/biến mất để reset làn cho anchor đó (tuỳ chọn).</summary>
    public static void ResetLanes(Transform anchor)
    {
        if (anchor) _laneIndex.Remove(anchor);
    }
}
