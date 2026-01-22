using UnityEngine;
using UnityEngine.Playables;

public class SceneCutsceneAutoStart : MonoBehaviour
{
    public CutsceneController controller;   // KÉO từ Hierarchy
    public PlayableDirector director;       // KÉO từ Hierarchy
    public bool onlyWhenNewGame = false;    // tắt = luôn chạy để test

    void Start()
    {
        Debug.Log("[AutoStart] Start()");
        if (onlyWhenNewGame && PlayerPrefs.GetInt("PlayPrologue", 0) != 1)
        {
            Debug.Log("[AutoStart] Skip (not New Game)");
            return;
        }

        if (!controller) { Debug.LogError("[AutoStart] controller == null"); return; }
        if (director) director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

        controller.PlayCutscene();          // <— GỌI Ở ĐÂY
        PlayerPrefs.SetInt("PlayPrologue", 0); // dọn cờ nếu bạn dùng New Game
        PlayerPrefs.Save();
    }
}
