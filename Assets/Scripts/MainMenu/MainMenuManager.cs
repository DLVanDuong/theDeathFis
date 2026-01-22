using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Config")]
    public string mainGameSceneName = "NewGame";

    [Header("UI Refs")]
    public GameObject settingsPanel;
    public Button continueButton;

    private const string GUIDE_FLAG = "SHOW_CONTROLS_GUIDE";

    void Start()
    {
        // bật/tắt Continue dựa trên save bằng SaveSystem
        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasKey("player_level");

        AudioManager.Instance?.PlayMusic(AudioManager.Instance.bgmNormal, true, 0.7f);
    }

    public void StartNewGame()
    {
        PlayerPrefs.SetInt("LoadGame", 0);
        PlayerPrefs.SetInt("PlayPrologue", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(mainGameSceneName);
        Debug.Log("Bắt đầu màn chơi mới!");
    }

    public void LoadGame()
    {
        // chỉ cần biết là có save không; còn lại sẽ khôi phục trong scene chơi
        if (SaveSystem.HasKey("player_level"))
        {
            PlayerPrefs.SetInt("LoadGame", 1);
            PlayerPrefs.SetInt(GUIDE_FLAG, 0);
            PlayerPrefs.Save();

            SceneManager.LoadScene(mainGameSceneName);
            Debug.Log("Đã chuẩn bị tải game (SaveSystem).");
        }
    }

    public void OpenOptions() { if (settingsPanel) settingsPanel.SetActive(true); }
    public void CloseOptions() { if (settingsPanel) settingsPanel.SetActive(false); }
    public void QuitGame() { Debug.Log("Đã thoát game!"); Application.Quit(); }
}
