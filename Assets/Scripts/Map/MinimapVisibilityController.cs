using System.Collections.Generic;
using UnityEngine;

public class MinimapVisibilityController : MonoBehaviour
{
    [Header("Minimap Root (object muốn tắt/bật)")]
    public GameObject minimapRoot;          // ví dụ: Minimap_BG (hoặc cả group minimap)

    [Header("Cutscene")]
    public bool startHiddenUntilCutsceneDone = true; // vào game ẩn minimap
    public bool cutsceneDone = false;                // set true khi cut xong

    [Header("Hide minimap when any of these panels are open")]
    public List<GameObject> hideWhenOpen = new();    // kéo các panel túi, stats, shop, rèn...

    [Header("Force close these panels when cutscene ends")]
    public List<GameObject> forceCloseOnCutsceneEnd = new();
    bool lastVisible;

    void Awake()
    {
        if (!minimapRoot) minimapRoot = gameObject;

        if (startHiddenUntilCutsceneDone)
            cutsceneDone = false;

        Apply(false, force: true);
    }

    void Start()
    {
        EvaluateAndApply();
    }

    void Update()
    {
        EvaluateAndApply();
    }

    void EvaluateAndApply()
    {
        bool anyOpen = IsAnyPanelOpen();

        bool shouldShow = (!startHiddenUntilCutsceneDone || cutsceneDone) && !anyOpen;

        Debug.Log($"[Minimap] cutsceneDone={cutsceneDone} anyOpen={anyOpen} => show={shouldShow}");

        Apply(shouldShow, true); // luôn force
    }

    bool IsAnyPanelOpen()
    {
        for (int i = 0; i < hideWhenOpen.Count; i++)
        {
            var go = hideWhenOpen[i];
            if (go == null || !go.activeInHierarchy) continue;

            var cg = go.GetComponentInChildren<CanvasGroup>(true);
            if (cg != null)
            {
                if (cg.alpha > 0.01f) return true; // thật sự đang hiện
                else continue; // active nhưng đang ẩn -> bỏ qua
            }

            return true; // không có CanvasGroup, active = mở
        }
        return false;
    }

    void Apply(bool visible, bool force = false)
    {
        Debug.Log($"[Minimap] Apply visible={visible}");

        if (minimapRoot)
        {
            minimapRoot.SetActive(visible);
        }

        lastVisible = visible;
    }

    // ======= GỌI TỪ CUTSCENE / TIMELINE =======
    public void OnCutsceneFinished()
    {
        cutsceneDone = true;

        // ✅ Tắt các panel đang bị bật ngầm
        for (int i = 0; i < forceCloseOnCutsceneEnd.Count; i++)
        {
            var go = forceCloseOnCutsceneEnd[i];
            if (go) go.SetActive(false);
        }

        EvaluateAndApply();
    }

    // Nếu muốn gọi để ẩn (khi mở UI đặc biệt)
    public void ForceHide() => Apply(false, force: true);

    // Nếu muốn gọi để hiện lại
    public void ForceShow()
    {
        cutsceneDone = true;
        Apply(true, force: true);
    }
}