using UnityEngine;
using TMPro;
using System.Collections;

public class CutsceneUI : MonoBehaviour
{
    public CanvasGroup group;
    public TMP_Text subtitle;
    [Range(10f, 120f)] public float charsPerSecond = 40f;

    private Coroutine typing;

    public IEnumerator Fade(float target, float dur)
    {
        float t = 0f;
        float start = group ? group.alpha : 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;              // chạy cả khi timeScale=0
            if (group) group.alpha = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        if (group) group.alpha = target;
    }

    public Coroutine PlayLine(string text, float hold)
    {
        if (!isActiveAndEnabled) return null;
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(PlayLineCo(text, hold));
        return typing;
    }

    private IEnumerator PlayLineCo(string text, float hold)
    {
        if (subtitle) subtitle.text = "";
        if (string.IsNullOrEmpty(text)) yield break;

        float cps = Mathf.Max(1f, charsPerSecond);
        foreach (char c in text)
        {
            if (subtitle) subtitle.text += c;
            yield return new WaitForSecondsRealtime(1f / cps); // unscaled
        }
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, hold)); // unscaled
    }

    public void Clear()
    {
        if (subtitle) subtitle.text = "";
    }
}
