using UnityEngine;

[RequireComponent(typeof(ZoneArea))]
public class ZoneArea_MinimapBridge : MonoBehaviour
{
    private ZoneArea zone;

    void Awake()
    {
        zone = GetComponent<ZoneArea>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        MinimapZoneLabel.Instance?.Show(zone.zoneName, zone.minLevel, zone.maxLevel);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        MinimapZoneLabel.Instance?.Hide();
    }

    void Start()
    {
        // Fix trường hợp player đã ở sẵn trong zone khi load scene
        Invoke(nameof(CheckPlayerInside), 0.1f);
    }

    void CheckPlayerInside()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        if (zone.Contains(player.transform.position))
            MinimapZoneLabel.Instance?.Show(zone.zoneName, zone.minLevel, zone.maxLevel);
    }
}