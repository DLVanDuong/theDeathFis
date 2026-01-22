using UnityEngine;
public class BGMStarter : MonoBehaviour
{
    void Start() => AudioManager.Instance?.PlayMusic(AudioManager.Instance.bgmNormal, true, 0.7f);
}
