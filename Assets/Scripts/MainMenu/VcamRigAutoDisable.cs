using UnityEngine;
using System.Collections;

public class VcamRigAutoDisable : MonoBehaviour
{
    private void Awake()
    {
        // Nếu load từ continue, tắt luôn trước khi Timeline kích hoạt
        if (PlayerPrefs.GetInt("LoadGame", 0) == 1)
        {
            gameObject.SetActive(false);
            Debug.Log("🎥 [VcamRigAutoDisable] Tắt VcamRig ngay từ Awake (LoadGame).");
        }
    }

    IEnumerator Start()
    {
        // Backup: đợi 1 frame, nếu vẫn bật thì tắt lại
        yield return null;

        if (PlayerPrefs.GetInt("LoadGame", 0) == 1 && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            Debug.Log("🎥 [VcamRigAutoDisable] Đã tắt VcamRig trong Start (backup).");
        }
    }
}
