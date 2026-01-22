using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    public ConsumableData itemData;

    [Header("Tự biến mất sau")]
    [Tooltip("Thời gian tồn tại tối đa trên mặt đất (giây)")]
    public float lifetime = 30f; // 30 giây
    [Tooltip("Khoảng thời gian trước khi biến mất để bắt đầu chớp nháy (giây)")]
    public float blinkDuration = 3f;

    private float spawnTime;
    private bool isBlinking = false;
    private Renderer[] renderers;

    private void Awake()
    {
        spawnTime = Time.time;
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Update()
    {
        float elapsed = Time.time - spawnTime;
        float remaining = lifetime - elapsed;

        // Bắt đầu chớp nháy ở vài giây cuối
        if (!isBlinking && remaining <= blinkDuration && remaining > 0)
        {
            StartCoroutine(BlinkEffect());
            isBlinking = true;
        }

        // Hết thời gian thì mờ dần rồi biến mất
        if (elapsed >= lifetime)
        {
            StartCoroutine(FadeAndDestroy());
        }
    }

    private IEnumerator BlinkEffect()
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

    private IEnumerator FadeAndDestroy()
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
        Destroy(gameObject);
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        QuickSlotManager qsm = other.GetComponent<QuickSlotManager>();
        if (qsm != null)
        {
            if (qsm.AddConsumable(itemData))
            {
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.pickup, 1f);
                Destroy(gameObject); // nhặt xong biến mất
            }
        }
    }
}
