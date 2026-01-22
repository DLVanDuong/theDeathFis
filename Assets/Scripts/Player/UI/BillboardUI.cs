using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    public Transform anchor; // head bone (để trống dùng root)
    public Vector3 worldOffset = new Vector3(0, 2.4f, 0);
    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (!anchor) anchor = GetComponentInParent<EnemyStateMachine>()?.transform;
    }

    void LateUpdate()
    {
        if (!anchor) return;
        transform.position = anchor.position + worldOffset;

        if (cam)
        {
            Vector3 dir = (transform.position - cam.transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}
