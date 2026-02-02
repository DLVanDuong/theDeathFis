using TMPro;
using UnityEngine;

public class StoneUI : MonoBehaviour
{
    [Header("Text hiển thị đá nâng cấp")]
    public TextMeshProUGUI stone0to5Text;
    public TextMeshProUGUI stone5to10Text;

    [Header("Crafting stones (tuỳ chọn)")]
    public TextMeshProUGUI catalystText; // Stone_Element
    public TextMeshProUGUI windText;
    public TextMeshProUGUI thunderText;
    public TextMeshProUGUI fireText;
    public TextMeshProUGUI earthText;

    private void Start()
    {
        Refresh();

        if (Inventory.Instance != null)
            Inventory.Instance.OnChanged += Refresh;
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        if (Inventory.Instance == null) return;

        if (stone0to5Text != null)
            stone0to5Text.text = Inventory.Instance.stone0to5.ToString();

        if (stone5to10Text != null)
            stone5to10Text.text = Inventory.Instance.stone5to10.ToString();

        if (catalystText != null)
            catalystText.text = Inventory.Instance.stoneElement.ToString();

        if (windText != null)
            windText.text = Inventory.Instance.stoneWind.ToString();

        if (thunderText != null)
            thunderText.text = Inventory.Instance.stoneThunder.ToString();

        if (fireText != null)
            fireText.text = Inventory.Instance.stoneFire.ToString();

        if (earthText != null)
            earthText.text = Inventory.Instance.stoneEarth.ToString();
    }
}
