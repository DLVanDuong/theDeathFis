using UnityEngine;
using UnityEngine.AI;

public class ZoneArea : MonoBehaviour
{
    [Header("Zone Info")]
    public string zoneName = "Zone";
    public int minLevel = 1;
    public int maxLevel = 5;
    public float radius = 20f;

    public bool Contains(Vector3 pos)
    {
        Vector3 c = transform.position;
        pos.y = c.y;
        return Vector3.Distance(pos, c) <= radius;
    }

    public Vector3 GetRandomPoint()
    {
        Vector2 r = UnityEngine.Random.insideUnitCircle * radius;
        return transform.position + new Vector3(r.x, 0f, r.y);
    }

    public Vector3 SampleOnNavMesh(Vector3 desired, float maxDist = 4f)
    {
        if (NavMesh.SamplePosition(desired, out var hit, maxDist, NavMesh.AllAreas))
            return hit.position;

        // fallback
        Vector3 candidate = GetRandomPoint();
        return NavMesh.SamplePosition(candidate, out hit, radius, NavMesh.AllAreas)
             ? hit.position : desired;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ZoneInfoUI.Instance?.ShowZone(zoneName, minLevel, maxLevel);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ZoneInfoUI.Instance?.HideZone();
        }
    }
    void Start()
    {
        Invoke(nameof(CheckPlayerInside), 0.1f); // đợi 1 frame để UI kịp Awake
    }

    void CheckPlayerInside()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, transform.localScale / 2f);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                ZoneInfoUI.Instance?.ShowZone(zoneName, minLevel, maxLevel);
                return;
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.8f, 0.2f, 0.25f);
        Gizmos.DrawSphere(transform.position, 0.25f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.4f, $"{zoneName}  Lv.{minLevel}-{maxLevel}");
    }
#endif
}
