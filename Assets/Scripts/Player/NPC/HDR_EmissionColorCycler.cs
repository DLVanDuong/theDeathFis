using UnityEngine;

[DisallowMultipleComponent]
public class HDR_EmissionColorCycler : MonoBehaviour
{
    [Header("Target")]
    public Renderer targetRenderer;      // kéo MeshRenderer vào (nếu để trống sẽ tự lấy trên object)
    public Material targetMaterial;      // nếu bạn muốn chỉ định material cụ thể (khuyên dùng)

    [Header("Emission")]
    public string emissionColorProperty = "_EmissionColor";
    public float intensity = 3f;         // độ sáng HDR (tăng lên nếu muốn chói hơn)
    public float speed = 0.25f;          // tốc độ đổi màu

    [Header("Options")]
    public bool useInstancedMaterial = true; // true: chỉ đổi trên object này, không làm đổi asset material gốc

    Material _mat;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();

        // Ưu tiên material do bạn kéo vào
        if (targetMaterial != null)
        {
            _mat = targetMaterial;
        }
        else if (targetRenderer != null)
        {
            // material: tạo instance riêng cho object (không đổi hàng loạt)
            // sharedMaterial: đổi luôn asset material (mọi object dùng chung bị đổi)
            _mat = useInstancedMaterial ? targetRenderer.material : targetRenderer.sharedMaterial;
        }

        if (_mat == null)
        {
            Debug.LogError("[HDR_EmissionColorCycler] Không tìm thấy Material để đổi màu.");
            enabled = false;
            return;
        }

        // Bật keyword emission cho URP/Lit
        _mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        if (_mat == null) return;

        // Hue chạy 0..1 theo thời gian
        float h = Mathf.Repeat(Time.time * speed, 1f);

        // Màu đổi liên tục (HSV -> RGB)
        Color rgb = Color.HSVToRGB(h, 1f, 1f);

        // HDR emission = màu * intensity
        Color hdr = rgb * intensity;

        _mat.SetColor(emissionColorProperty, hdr);
    }
}
