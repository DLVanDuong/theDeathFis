using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;
    public float height = 50f;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 pos = player.position;
        pos.y = height;

        transform.position = pos;
    }
}