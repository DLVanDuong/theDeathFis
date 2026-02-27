using UnityEngine;
using UnityEngine.Playables;
using System;
using System.Collections;
using System.Collections.Generic;

public class CutsceneController : MonoBehaviour
{
    [Serializable]
    public class DialogueLine
    {
        [TextArea] public string text;
        public float atTime = 0f;
        public float hold = 2.5f;
    }

    public PlayableDirector director;
    public CutsceneUI ui;
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Toggle objects with cutscene")]
    public GameObject[] activateOnStart;
    public GameObject[] deactivateOnEnd; // <- chứa VcamRig

    [Header("Gameplay lock")]
    public GameObject[] disableObjects;

    [Header("Skip / Exit")]
    public KeyCode skipKey = KeyCode.Space;
    public bool showGuideAfterWhenSkipped = false;

    [Header("Guide (nếu bạn đã có phần này, giữ nguyên)")]
    public GameObject guidePanel;
    public bool showGuideAfter = false;
    public KeyCode guideCloseKey = KeyCode.Space;
    public bool pauseWhileGuide = true;
    public float guideFade = 0.25f;

    public bool deactivateAfter = true;

    private bool playing = false;
    private bool wasSkipped = false;


    // 🟢 THÊM CALLBACK TẮT CAMERA CUTSCENE
    void Awake()
    {
        if (director != null)
            director.stopped += OnCutsceneEnd;
    }

    void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnCutsceneEnd;
    }

    // 🧩 Hàm này đảm bảo dù skip hoặc load lại đều tắt VcamRig
    private void OnCutsceneEnd(PlayableDirector d)
    {
        foreach (var go in deactivateOnEnd)
            if (go) go.SetActive(false);
    }

    void Update()
    {
        if (!playing) return;

        if (Input.GetKeyDown(skipKey))
        {
            wasSkipped = true;
            if (director != null)
            {
                director.time = director.duration;
                director.Evaluate();
                director.Stop(); // 🟢 đảm bảo trigger OnCutsceneEnd
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("TEST ▶ PlayCutscene Now")]
    void Editor_PlayNow() { PlayCutscene(); }
#endif

    public void PlayCutscene()
    {
        if (PlayerPrefs.GetInt("LoadGame", 0) == 1)
        {
            // ⚡ Bỏ qua cutscene hoàn toàn khi load game
            gameObject.SetActive(false);
            return;
        }
        // ⚙️ Nếu là Load Game → bỏ qua cutscene
        if (PlayerPrefs.GetInt("LoadGame", 0) == 1)
        {
            foreach (var go in deactivateOnEnd)
                if (go) go.SetActive(false);
            FindAnyObjectByType<MinimapVisibilityController>()?.OnCutsceneFinished();
            gameObject.SetActive(false);
            return;
        }

        if (playing) return;
        playing = true;
        wasSkipped = false;

        foreach (var go in disableObjects) if (go) go.SetActive(false);
        foreach (var go in activateOnStart) if (go) go.SetActive(true);

        if (ui)
        {
            ui.gameObject.SetActive(true);
            StartCoroutine(ui.Fade(1f, 0.25f));
        }

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (director)
        {
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.time = 0;
            director.Play();
        }

        foreach (var L in lines)
        {
            if (wasSkipped) break;

            if (director)
            {
                while (director.state == PlayState.Playing && director.time < L.atTime)
                {
                    if (wasSkipped) break;
                    yield return null;
                }
            }
            else
            {
                if (wasSkipped) break;
                yield return new WaitForSecondsRealtime(Mathf.Max(0f, L.atTime));
            }

            if (wasSkipped) break;
            if (ui) yield return ui.PlayLine(L.text, L.hold);
        }

        if (director)
            while (director.state == PlayState.Playing && !wasSkipped)
                yield return null;

        if (ui)
        {
            ui.Clear();
            yield return ui.Fade(0f, 0.25f);
            ui.gameObject.SetActive(false);
        }

        foreach (var go in disableObjects) if (go) go.SetActive(true);
        FindAnyObjectByType<MinimapVisibilityController>()?.OnCutsceneFinished();

        yield return null;
        foreach (var go in deactivateOnEnd) if (go) go.SetActive(false);

        bool shouldShowGuide = showGuideAfter && (!wasSkipped || showGuideAfterWhenSkipped);
        if (shouldShowGuide && guidePanel)
        {
            var cg = guidePanel.GetComponent<CanvasGroup>();
            guidePanel.SetActive(true);
            if (cg) { cg.interactable = true; cg.blocksRaycasts = true; cg.alpha = 0f; yield return FadeCanvasGroup(cg, 0f, 1f, guideFade); }

            if (pauseWhileGuide) Time.timeScale = 0f;
            while (!Input.GetKeyDown(guideCloseKey) && !Input.GetKeyDown(KeyCode.Escape))
                yield return null;
            if (pauseWhileGuide) Time.timeScale = 1f;

            if (cg) { cg.interactable = false; cg.blocksRaycasts = false; yield return FadeCanvasGroup(cg, 1f, 0f, guideFade); }
            guidePanel.SetActive(false);
        }

        playing = false;
        if (deactivateAfter) gameObject.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup g, float a, float b, float dur)
    {
        if (!g) yield break;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(a, b, t / dur);
            yield return null;
        }
        g.alpha = b;
    }
}
