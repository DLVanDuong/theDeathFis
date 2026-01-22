using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    private Camera cam;

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // quay mặt về camera, giữ hướng "đứng"
        Vector3 dir = transform.position - cam.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
