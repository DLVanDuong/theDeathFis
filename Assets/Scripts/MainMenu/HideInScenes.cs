using UnityEngine;
using UnityEngine.SceneManagement;

public class HideInScenes : MonoBehaviour
{
    [SerializeField] string[] scenesToHide = { "ManMenu" };

    void OnEnable() { SceneManager.sceneLoaded += OnLoaded; Check(); }
    void OnDisable() { SceneManager.sceneLoaded -= OnLoaded; }

    void OnLoaded(Scene s, LoadSceneMode m) => Check();

    void Check()
    {
        string cur = SceneManager.GetActiveScene().name;
        bool hide = System.Array.Exists(scenesToHide, x => x == cur);
        gameObject.SetActive(!hide);
    }
}
