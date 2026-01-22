using UnityEngine;
using System.Collections;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin")]
    public int amount = 1;

    [Header("Tự biến mất sau")]
    public float lifetime = 30f;
    public float blinkDuration = 3f;

    private float spawnTime;
    private bool isBlinking = false;
    private Renderer[] renderers;

    void Awake()
    {
        spawnTime = Time.time;
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    void Update()
    {
        float elapsed = Time.time - spawnTime;
        float remaining = lifetime - elapsed;

        if (!isBlinking && remaining <= blinkDuration && remaining > 0)
        {
            StartCoroutine(BlinkEffect());
            isBlinking = true;
        }

        if (elapsed >= lifetime)
        {
            StartCoroutine(FadeAndDestroy());
        }
    }

    IEnumerator BlinkEffect()
    {
        float blinkSpeed = 0.2f;
        while (Time.time - spawnTime < lifetime)
        {
            SetRenderersVisible(false);
            yield return new WaitForSeconds(blinkSpeed);
            SetRenderersVisible(true);
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    IEnumerator FadeAndDestroy()
    {
        float fadeTime = 0.5f;
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / fadeTime);

            foreach (var r in renderers)
            {
                if (r != null && r.material.HasProperty("_Color"))
                {
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;
                }
            }
            yield return null;
        }
        Destroy(transform.root.gameObject); // 🔥 XOÁ CẢ COIN
    }

    void SetRenderersVisible(bool visible)
    {
        foreach (var r in renderers)
            if (r != null) r.enabled = visible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var wallet = other.GetComponent<PlayerWallet>();
        if (wallet == null) wallet = PlayerWallet.Instance;

        if (wallet != null)
        {
            wallet.AddCoin(amount);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.pickup, 1f);
            Destroy(transform.root.gameObject); // 🔥 XOÁ ROOT
        }
    }
}
