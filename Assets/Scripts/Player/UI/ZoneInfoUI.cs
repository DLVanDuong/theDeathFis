using UnityEngine;
using TMPro;
using System.Collections;

public class ZoneInfoUI : MonoBehaviour
{
    public static ZoneInfoUI Instance;

    public TextMeshProUGUI zoneNameText;
    public TextMeshProUGUI zoneLevelText;
    public CanvasGroup canvasGroup;

    private Coroutine fadeRoutine;

    // ===== Cache zone hiện tại để bật lại sau khi đóng UI =====
    private string lastZoneName = "";
    private int lastMinLv = 0;
    private int lastMaxLv = 0;
    private bool hasLastZone = false;

    // Đánh dấu đang bị UI lớn che (Inventory/Pause)
    private bool suppressedByOverlay = false;

    void Awake()
    {
        Instance = this;
        canvasGroup.alpha = 0;
    }

    public void ShowZone(string zoneName, int minLv, int maxLv)
    {
        // Lưu lại zone gần nhất
        lastZoneName = zoneName;
        lastMinLv = minLv;
        lastMaxLv = maxLv;
        hasLastZone = true;

        // Nếu đang bị Inventory/Pause che thì KHÔNG hiện
        if (suppressedByOverlay) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        // ===== TEXT =====
        zoneNameText.text = zoneName;
        zoneLevelText.text = $"Cấp đề nghị: {minLv} – {maxLv}";

        // ===== LẤY LEVEL PLAYER =====
        int playerLevel = 1;
        var levelSys = FindAnyObjectByType<PlayerLevelSystem>();
        if (levelSys != null)
            playerLevel = levelSys.playerStats.level;

        // ===== ĐỊNH NGHĨA MÀU =====
        Color colorRed = new Color(1f, 0.25f, 0.25f);   // zone quá cao
        Color colorYellow = new Color(1f, 0.85f, 0.2f);   // đúng tầm
        Color colorGreen = new Color(0.3f, 1f, 0.3f);    // zone thấp

        Color finalColor;

        // ===== LOGIC SO MÀU =====
        if (playerLevel < minLv)
        {
            finalColor = colorRed;        // 🔴 nguy hiểm
        }
        else if (playerLevel > maxLv)
        {
            finalColor = colorGreen;      // 🟢 quá dễ
        }
        else
        {
            finalColor = colorYellow;     // 🟡 phù hợp
        }

        zoneNameText.color = finalColor;
        zoneLevelText.color = finalColor;

        fadeRoutine = StartCoroutine(Fade(1));
    }


    public void HideZone()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(0));
    }

    // Gọi khi mở Inventory/Pause
    public void Suppress(bool hideImmediate = true)
    {
        suppressedByOverlay = true;
        if (hideImmediate) ForceHideImmediate();
        else HideZone();
    }

    // Gọi khi tắt Inventory/Pause
    public void UnsuppressAndRestore()
    {
        suppressedByOverlay = false;

        // Nếu có zone trước đó thì hiện lại
        if (hasLastZone)
            ShowZone(lastZoneName, lastMinLv, lastMaxLv);
    }

    private IEnumerator Fade(float target)
    {
        float start = canvasGroup.alpha;
        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, time / 0.5f);
            yield return null;
        }
        canvasGroup.alpha = target;
    }

    public void ForceHideImmediate()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        canvasGroup.alpha = 0;
    }
}
