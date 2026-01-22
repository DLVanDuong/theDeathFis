using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TintZone : MonoBehaviour
{
    [Header("Tint")]
    [Tooltip("Mã hex ví dụ #EE9A9A hoặc EE9A9A")]
    public string hexColor = "#EE9A9A";
    [Range(0, 1f)] public float alpha = 0.45f;
    public int priority = 0;        // nếu 2 vùng chồng lên nhau, vùng priority cao hơn sẽ được áp dụng

    [Header("Fade")]
    public float fadeIn = 0.25f;
    public float fadeOut = 0.25f;

    private Color color;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void Awake()
    {
       
        string hex = hexColor.StartsWith("#") ? hexColor : ("#" + hexColor);
        if (ColorUtility.TryParseHtmlString(hex, out var c))
        {
            color = c;               // lưu màu
            if (hex.Length == 9)     // có kèm AA
                alpha = c.a;         // dùng alpha trong hex
            color.a = 1f;            // còn alpha sẽ lấy từ 'alpha' riêng
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!ScreenTintOverlay.Instance)
            new GameObject("ScreenTintOverlay").AddComponent<ScreenTintOverlay>(); // tự tạo khi cần

        ScreenTintOverlay.Instance.Register(this, color, alpha, priority, fadeIn);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (ScreenTintOverlay.Instance)
            ScreenTintOverlay.Instance.Unregister(this, fadeOut);
    }
}
