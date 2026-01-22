using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public static bool IsPlayerNearShop { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            IsPlayerNearShop = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            IsPlayerNearShop = false;
    }
}
