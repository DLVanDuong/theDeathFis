using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PauseMenuUI : MonoBehaviour
{
    public GameObject pausePanel;    // Kéo Panel PauseMenu vào
    public Button saveButton;
    public Button continueButton;
    public Button quitButton;

    [Header("UI Optional")]
    public TextMeshProUGUI saveMessageText;

    [Header("Scene Config")]
    public string mainMenuSceneName = "ManMenu"; // đặt đúng tên scene menu chính

    [Header("Tint Toggle (simple)")]
    [SerializeField] private GameObject tintGO;   // KÉO object Image "Tint" vào đây
    private bool tintWasActive;

    private PlayerStateMachine player;

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
        player = FindAnyObjectByType<PlayerStateMachine>();

        if (saveButton) saveButton.onClick.AddListener(OnSaveGame);
        if (continueButton) continueButton.onClick.AddListener(OnContinue);
        if (quitButton) quitButton.onClick.AddListener(OnQuit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseMenu();
    }

    void TogglePauseMenu()
    {
        bool isActive = !pausePanel.activeSelf;
        pausePanel.SetActive(isActive);

        if (isActive)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (tintGO)
            {
                tintWasActive = tintGO.activeSelf; // nhớ trạng thái
                tintGO.SetActive(false);           // tắt tint khi mở pause
            }
            if (ZoneInfoUI.Instance != null)
                ZoneInfoUI.Instance.Suppress(true);
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (tintGO) tintGO.SetActive(tintWasActive); // trả về như cũ

            if (ZoneInfoUI.Instance != null)
                ZoneInfoUI.Instance.UnsuppressAndRestore();
        }
    }

    void OnSaveGame()
    {
        // Tìm SaveQuick trong scene (có thể gán qua Inspector cũng được)
        var saver = FindAnyObjectByType<SaveQuick>();
        if (saver != null)
        {
            saver.SaveNow();
            Debug.Log("Đã lưu game (SaveSystem)!");

            if (saveMessageText != null)
                StartCoroutine(ShowSaveMessage());
        }
        else
        {
            Debug.LogWarning("Không tìm thấy SaveQuick trong scene!");
        }
    }

    IEnumerator ShowSaveMessage()
    {
        saveMessageText.gameObject.SetActive(true);
        saveMessageText.text = "Đã lưu game!";
        yield return new WaitForSecondsRealtime(2f);
        saveMessageText.gameObject.SetActive(false);
    }

    void OnContinue() => TogglePauseMenu();

    void OnQuit()
    {
        var pauseCanvas = GameObject.Find("PauseMenuCanvas");
        if (pauseCanvas) Destroy(pauseCanvas);
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
        void OnDestroy()
    {
        if (tintGO) tintGO.SetActive(tintWasActive); // (bảo hiểm)
    }
}
