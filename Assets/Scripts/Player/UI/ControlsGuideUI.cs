using UnityEngine;

public class ControlsGuideUI : MonoBehaviour
{
    public CanvasGroup group;                 // kéo CanvasGroup của Panel vào
    public KeyCode toggleKey = KeyCode.H;     // phím bật/tắt
    const string KEY = "SHOW_CONTROLS_GUIDE"; // cờ từ menu

    void Reset() { group = GetComponent<CanvasGroup>(); }

    void Start()
    {
        // Nếu New Game vừa set cờ -> hiện panel
        if (PlayerPrefs.GetInt(KEY, 0) == 1)
        {
            Show();
            PlayerPrefs.SetInt(KEY, 0); // dùng xong thì tắt cờ
        }
        else
        {
            HideImmediate();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Toggle()
    {
        if (group.alpha > 0.5f) HideImmediate();
        else Show();
    }

    public void Show()
    {
        if (!group) return;
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = true;
    }

    public void HideImmediate()
    {
        if (!group) return;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }
}
