using UnityEngine;

public class WeaponGlowController : MonoBehaviour
{
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    [Header("Perf")]
    [SerializeField] float animateFps = 20f;      // +10 sẽ cập nhật 20 FPS
    [SerializeField] float cycleDuration = 2.4f;  // thời gian chuyển 3 màu

    Renderer[] rends;
    MaterialPropertyBlock mpb;
    bool animate;
    float tick;
    Color baseColor;
    Color[] cycle;
    float intensity = 2f;

    public void Setup(int upgradeLevel, Color rarityColor)
    {
        if (rends == null) rends = GetComponentsInChildren<Renderer>(true);
        if (mpb == null) mpb = new MaterialPropertyBlock();

        EnableEmissionKeywords();

        // mặc định: nhẹ nhàng
        baseColor = rarityColor;
        intensity = 1.2f;

        if (upgradeLevel >= 10)
        {
            // 3 màu — bạn có thể đổi bộ màu cho hợp style
            cycle = new[] { new Color(1f, 0.5f, 0.2f), new Color(0.7f, 0.4f, 1f), new Color(0.3f, 0.9f, 1f) }; // cam → tím → cyan
            intensity = Mathf.Lerp(2.2f, 3.5f, Mathf.InverseLerp(10, 20, upgradeLevel));
            animate = true;
        }
        else if (upgradeLevel >= 5)
        {
            // 1 màu (static) — dùng luôn màu phẩm cho đồng bộ UI
            intensity = Mathf.Lerp(1.6f, 2.6f, Mathf.InverseLerp(5, 9, upgradeLevel));
            animate = false;
            ApplyColor(baseColor);
        }
        else
        {
            // +0..+4: rất nhẹ
            intensity = 0.9f;
            animate = false;
            ApplyColor(baseColor);
        }
    }

    void EnableEmissionKeywords()
    {
        foreach (var r in rends)
        {
            if (!r) continue;
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m && m.HasProperty(EmissionColorID))
                    m.EnableKeyword("_EMISSION");
            }
        }
    }

    void ApplyColor(Color c)
    {
        // dùng HDR linear để bloom đẹp
        Color hdr = c.linear * intensity;
        foreach (var r in rends)
        {
            if (!r) continue;
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColorID, hdr);
            r.SetPropertyBlock(mpb);
        }
    }

    void Update()
    {
        if (!animate || cycle == null || cycle.Length < 3) return;

        tick += Time.unscaledDeltaTime;
        float step = 1f / Mathf.Max(1f, animateFps);
        if (tick < step) return;
        tick = 0f;

        float t = Mathf.Repeat(Time.time / cycleDuration, 1f);
        // lerp qua 3 màu: [0..1/3], [1/3..2/3], [2/3..1]
        float seg = t * 3f;
        int i = Mathf.FloorToInt(seg) % 3;
        int j = (i + 1) % 3;
        float u = seg - Mathf.Floor(seg);

        Color c = Color.Lerp(cycle[i], cycle[j], u);
        ApplyColor(c);
    }
}
