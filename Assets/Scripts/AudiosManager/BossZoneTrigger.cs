using UnityEngine;
public class BossZoneTrigger : MonoBehaviour
{
    bool entered;
    void OnTriggerEnter(Collider other)
    {
        if (entered || !other.CompareTag("Player")) return;
        entered = true;
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.bgmBoss, true, 0.9f);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.bossRoar, 1f);
    }
}
