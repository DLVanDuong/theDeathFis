using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource musicSource; // NHẠC NÊN 
    public AudioSource sfxSource;   // HIỆU ỨNG ÂM THANH

    [Header("BGM")]
    public AudioClip bgmNormal; // nhạc nền bình thường 
    public AudioClip bgmBoss; // nhạc nền boss

    [Header("Player SFX")]
    public AudioClip footstepWalk;
    public AudioClip footstepRun;
    public AudioClip swordSwing;
    public AudioClip bowShoot;
    public AudioClip swordHitEnemy;   // trúng enemy

    [Header("World / UI SFX")]
    public AudioClip pickup;
    public AudioClip summonEnemy;
    public AudioClip bossRoar;

    [Header("Enemy / Boss Death SFX")]
    public AudioClip enemyDeathFlesh;   // quái có thịt chết
    public AudioClip enemyDeathBone;    // quái xương chết
    public AudioClip bossDeath;         // boss chết

    [Header("Equipment SFX")]
    public AudioClip equipItem;     // kéo thả file âm thanh equip vào đây
    public AudioClip unequipItem;   // kéo thả file âm thanh unequip vào đây

    [Header("Hit / Blood SFX")]
    public AudioClip playerHit;   // player bị đánh
    public AudioClip enemyHit;    // enemy bị đánh
    public AudioClip playerDeath; // player chết

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // ===== API =====
    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
    public void PlaySFXVaried(AudioClip clip, float volume = 1f, float pitchMin = 0.96f, float pitchMax = 1.04f)
    {
        if (clip == null) return;
        var temp = gameObject.AddComponent<AudioSource>();
        temp.clip = clip;
        temp.volume = volume;
        temp.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
        temp.Play();
        Destroy(temp, clip.length / Mathf.Max(0.01f, temp.pitch));
    }
    public void PlayEquipSFX(float volume = 1f) => PlaySFXVaried(equipItem, volume);
    public void PlayUnequipSFX(float volume = 1f) => PlaySFXVaried(unequipItem, volume);
    public void PlaySFXShort(AudioClip clip, float duration = 0.25f, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource temp = gameObject.AddComponent<AudioSource>();
        temp.clip = clip;
        temp.volume = volume;
        temp.Play();
        Destroy(temp, duration); // tự hủy audio sau X giây
    }
    // ===== Toggles =====
    public void ToggleMusic(bool on) { musicSource.mute = !on; }
    public void ToggleSFX(bool on) { sfxSource.mute = !on; }
    public void ToggleAll(bool on) { musicSource.mute = sfxSource.mute = !on; }

    // Optional: lưu cài đặt
    public void SetMusicVolume(float v) { musicSource.volume = v; PlayerPrefs.SetFloat("mus", v); }
    public void SetSFXVolume(float v) { sfxSource.volume = v; PlayerPrefs.SetFloat("sfx", v); }
}
