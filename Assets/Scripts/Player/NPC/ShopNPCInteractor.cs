using UnityEngine;

public class ShopNPCInteractor : MonoBehaviour
{
    [Header("Refs")]
    public ShopUIController shopUI;
    public GameObject hintPrefab;

    private bool playerInRange;
    private GameObject hintInstance;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TRIGGER ENTER: " + other.name + " tag=" + other.tag);

        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        Debug.Log("PLAYER ENTER SHOP ZONE");

        if (hintPrefab != null && hintInstance == null)
        {
            hintInstance = Instantiate(hintPrefab, transform);
            hintInstance.transform.localPosition = Vector3.up * 1.8f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        Debug.Log("PLAYER EXIT SHOP ZONE");

        if (hintInstance) Destroy(hintInstance);
    }

    private void Update()
    {
        if (!playerInRange) return;

        // Nhấn F để mở shop (ăn chắc, không phụ thuộc InputSystem)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("PRESS F OPEN SHOP");

            if (shopUI == null)
            {
                Debug.LogError("ShopUI chưa được gán!");
                return;
            }

            shopUI.Open();
        }
    }
}
