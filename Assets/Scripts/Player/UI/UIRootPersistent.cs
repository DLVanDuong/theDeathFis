using UnityEngine;

public class UIRootPersistent : MonoBehaviour
{
    private static UIRootPersistent _instance;
    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject); // vì nó là Root nên hợp lệ
    }
}
