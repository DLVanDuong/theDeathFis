using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ScreenTintOverlay : MonoBehaviour
{
    public static ScreenTintOverlay Instance { get; private set; }

    [Header("Overlay")]
    [SerializeField] private Image overlay;   // có thể kéo sẵn 1 Image full-screen vào đây
    [SerializeField] private int sortingOrder = 5000;
    [SerializeField] private bool createIfMissing = true;
    [Range(0f, 1f)] public float globalAlphaMultiplier = 1f;

    private Coroutine fadeCo;

    private struct Entry { public object owner; public Color color; public float alpha; public int priority; }
    private readonly List<Entry> active = new();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!overlay && createIfMissing)
            overlay = CreateOverlayUI();

        if (overlay) overlay.raycastTarget = false;
    }          
    void OnEnable()
    {
        if (overlay) overlay.transform.SetAsLastSibling();
    }
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private Image CreateOverlayUI()
    {
        var canvasGO = new GameObject("ScreenTintCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var imgGO = new GameObject("Tint", typeof(RectTransform), typeof(Image));
        imgGO.transform.SetParent(canvasGO.transform, false);

        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var img = imgGO.GetComponent<Image>();
        img.raycastTarget = false; // không chặn click UI khác
        img.color = new Color(0, 0, 0, 0);
        return img;
    }

    public void Register(object owner, Color color, float alpha, int priority, float fadeDuration = 0.2f)
    {
        active.RemoveAll(e => e.owner == owner);
        active.Add(new Entry { owner = owner, color = color, alpha = Mathf.Clamp01(alpha), priority = priority });
        ApplyTop(fadeDuration);
    }

    public void Unregister(object owner, float fadeDuration = 0.2f)
    {
        active.RemoveAll(e => e.owner == owner);
        ApplyTop(fadeDuration);
    }

    private void ApplyTop(float fadeDuration)
    {
        if (!overlay) return;

        if (active.Count == 0)
        {
            StartFade(new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0f), fadeDuration);
            return;
        }

        // lấy entry priority cao nhất (nếu bằng nhau lấy entry vào sau cùng)
        Entry top = active[0];
        for (int i = 1; i < active.Count; i++)
            if (active[i].priority >= top.priority) top = active[i];

        var c = top.color;
        c.a = Mathf.Clamp01(top.alpha * globalAlphaMultiplier);
        StartFade(c, fadeDuration);
    }

    private void StartFade(Color to, float duration)
    {
        if (!overlay) return;

        // Nếu object này đang inactive hoặc component disabled → set trực tiếp
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy || duration <= 0f)
        {
            overlay.color = to;
            return;
        }

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeRoutine(to, duration));
    }

    private IEnumerator FadeRoutine(Color to, float duration)
    {
        var img = overlay;
        Color from = img.color;
        if (duration <= 0f) { img.color = to; yield break; }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            img.color = Color.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        img.color = to;
    }
}
