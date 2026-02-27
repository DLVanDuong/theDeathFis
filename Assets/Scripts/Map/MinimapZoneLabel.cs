using TMPro;
using UnityEngine;

public class MinimapZoneLabel : MonoBehaviour
{
    public static MinimapZoneLabel Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI zoneText;

    [Header("Optional")]
    public bool hideWhenEmpty = true;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        ApplyEmpty();
    }

    public void Show(string zoneName, int minLv, int maxLv)
    {
        if (!zoneText) return;
        zoneText.text = $"{zoneName}  Lv.{minLv}-{maxLv}";
        if (hideWhenEmpty) zoneText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (!zoneText) return;
        zoneText.text = "";
        ApplyEmpty();
    }

    void ApplyEmpty()
    {
        if (!zoneText) return;
        if (hideWhenEmpty) zoneText.gameObject.SetActive(false);
    }
}