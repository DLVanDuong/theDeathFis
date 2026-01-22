using UnityEngine;

public class LoadOnSceneEnter : MonoBehaviour
{
    [SerializeField] LoadQuick loader;

    void Start()
    {
        if (PlayerPrefs.GetInt("LoadGame", 0) == 1)
        {
            if (loader == null) loader = FindAnyObjectByType<LoadQuick>();
            loader?.LoadNow();

            PlayerPrefs.SetInt("LoadGame", 0);
            PlayerPrefs.Save();
        }
    }
}
