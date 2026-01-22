using TMPro;
using UnityEngine;

public class ArrowUIManager : MonoBehaviour
{
    public static ArrowUIManager Instance;
    public TextMeshProUGUI messageText;
    public float showTime = 2f;

    private float timer;
    private bool showing;

    private void Awake()
    {
        Instance = this;
        if (messageText != null)
            messageText.text = "";
    }

    public void ShowArrowMessage(string msg)
    {
        if (messageText == null) return;
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        showing = true;
        timer = showTime;
    }

    private void Update()
    {
        if (!showing) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            messageText.text = "";
            showing = false;
        }
    }
}
