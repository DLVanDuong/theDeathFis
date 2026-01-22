using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;   // <- thêm dòng này

public class AudioSettingsUI : MonoBehaviour
{
    public Button musicBtn, sfxBtn, muteAllBtn;
    public Slider musicVolume, sfxVolume;
    bool musicOn = true, sfxOn = true, allOn = true;

    // GẮN LISTENER DYNAMIC = BẰNG CODE
    void Awake()
    {
        if (musicVolume)
        {
            musicVolume.minValue = 0f;
            musicVolume.maxValue = 1f;
            musicVolume.wholeNumbers = false;

            musicVolume.onValueChanged.RemoveAllListeners();   // xoá mọi thứ đã gán trong Inspector
            musicVolume.onValueChanged.AddListener(OnMusicVol); // <- Dynamic float CHẮC CHẮN
        }
        if (sfxVolume)
        {
            sfxVolume.minValue = 0f;
            sfxVolume.maxValue = 1f;
            sfxVolume.wholeNumbers = false;

            sfxVolume.onValueChanged.RemoveAllListeners();
            sfxVolume.onValueChanged.AddListener(OnSFXVol);     // <- Dynamic float
        }
    }

    void OnEnable()
    {
        // Đồng bộ UI với volume hiện tại mà KHÔNG kích hoạt event
        var am = AudioManager.Instance;
        if (am)
        {
            if (musicVolume) musicVolume.SetValueWithoutNotify(am.musicSource.volume);
            if (sfxVolume) sfxVolume.SetValueWithoutNotify(am.sfxSource.volume);
        }
        ApplyAll(); // nếu muốn áp lại toggle/mute
    }

    public void OnClickMusic() { musicOn = !musicOn; AudioManager.Instance?.ToggleMusic(musicOn); }
    public void OnClickSFX() { sfxOn = !sfxOn; AudioManager.Instance?.ToggleSFX(sfxOn); }
    public void OnClickAll() { allOn = !allOn; AudioManager.Instance?.ToggleAll(allOn); }

    public void OnMusicVol(float v)
    {
        Debug.Log("[AudioSettingsUI] MusicVol = " + v);
        AudioManager.Instance?.SetMusicVolume(v);
    }
    public void OnSFXVol(float v)
    {
        Debug.Log("[AudioSettingsUI] SFXVol = " + v);
        AudioManager.Instance?.SetSFXVolume(v);
    }

    void ApplyAll()
    {
        AudioManager.Instance?.ToggleMusic(musicOn);
        AudioManager.Instance?.ToggleSFX(sfxOn);
        AudioManager.Instance?.ToggleAll(allOn);

        // đặt mặc định lần đầu (nếu muốn): 
        if (musicVolume && AudioManager.Instance) AudioManager.Instance.SetMusicVolume(musicVolume.value);
        if (sfxVolume && AudioManager.Instance) AudioManager.Instance.SetSFXVolume(sfxVolume.value);
    }
}
