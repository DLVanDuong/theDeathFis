using UnityEngine;
using System.Collections;

public class CutsceneUISelfTest : MonoBehaviour
{
    public CutsceneUI ui;
    IEnumerator Start()
    {
        yield return null;
        ui.gameObject.SetActive(true);
        yield return ui.Fade(1f, 0.2f);
        yield return ui.PlayLine("TEST: UI OK — nhìn thấy dòng này là qua bước 2", 1.5f);
        yield return ui.Fade(0f, 0.2f);
    }
}
