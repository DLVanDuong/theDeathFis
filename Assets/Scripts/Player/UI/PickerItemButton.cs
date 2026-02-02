using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickerItemButton : MonoBehaviour
{
    public Button btn;
    public Image icon;
    public TextMeshProUGUI label;

    public void Set(Sprite s, string text, System.Action onClick)
    {
        if (icon)
        {
            icon.sprite = s;
            icon.enabled = (s != null);
        }
        if (label) label.text = text;

        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
