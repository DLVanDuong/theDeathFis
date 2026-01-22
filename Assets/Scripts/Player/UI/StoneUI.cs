using TMPro;
using UnityEngine;

public class StoneUI : MonoBehaviour
{
    [Header("Text hiển thị đá")]
    public TextMeshProUGUI stone0to5Text;
    public TextMeshProUGUI stone5to10Text;

    private void Start()
    {
        // Update lần đầu
        Refresh();

        // Lắng nghe inventory đổi để update UI
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
    }
}
