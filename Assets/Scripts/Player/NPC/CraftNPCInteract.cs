using UnityEngine;

public class CraftNPCInteract : MonoBehaviour
{
    [Header("Refs")]
    public CraftingUIController craftingUI;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.F;

    [Header("Trigger settings")]
    public string playerTag = "Player";

    private bool _playerInRange = false;
    private bool _isOpen = false;

    void Update()
    {
        if (!_playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (!_isOpen) OpenCraft();
            else CloseCraft();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInRange = true;
        // Debug.Log("Player in craft range");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInRange = false;

        // Nếu đi ra khỏi vùng mà UI vẫn mở thì đóng luôn cho chắc
        if (_isOpen) CloseCraft();
    }

    void OpenCraft()
    {
        if (!craftingUI) return;

        _isOpen = true;
        craftingUI.Toggle();

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseCraft()
    {
        if (!craftingUI) return;

        _isOpen = false;
        craftingUI.Toggle();

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
