using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 5.0f;
    public float heightOffset = 2.0f;
    public float mouseSensitivity = 100.0f;
    public float rotationSmoothTime = 0.12f;
    [Range(0f, 90f)] public float maxVerticalAngle = 80f;
    [Range(-90f, 0f)] public float minVerticalAngle = -80f;

    private float currentYaw = 0.0f;
    private float currentPitch = 0.0f;

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                target = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player object not found! Please assign a target for the camera.");
                enabled = false;
                return;
            }
        }

        Vector3 angles = transform.eulerAngles;
        currentYaw = angles.y;
        currentPitch = angles.x;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleCameraRotation();
        UpdateCameraPosition();
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredPosition = target.position + new Vector3(0, heightOffset, 0) - (rotation * Vector3.forward * distance);

        transform.position = desiredPosition;
        transform.LookAt(target.position + new Vector3(0, heightOffset, 0));
    }
}
