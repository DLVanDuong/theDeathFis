using UnityEngine;
using TMPro;

public class UpgradeStonePickup : MonoBehaviour
{
    public UpgradeStoneData stoneData;
    public int amount = 1;

    [Header("World Name Text")]
    [SerializeField] private TextMeshProUGUI worldNameText;

    public bool isDroppedFromEnemy = true;

    private void Awake()
    {
        if (worldNameText == null)
            worldNameText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (worldNameText != null)
        {
            worldNameText.gameObject.SetActive(isDroppedFromEnemy);
            RefreshName();
        }
    }

    private void RefreshName()
    {
        if (worldNameText == null || stoneData == null) return;

        string count = amount > 1 ? $" x{amount}" : "";
        worldNameText.text = stoneData.displayName + count;
        worldNameText.color = stoneData.nameColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (stoneData == null) return;

        Inventory.Instance.AddUpgradeStone(stoneData.stoneType, amount);
        InventoryUI.Instance?.RefreshUI();

        // ✅ Âm thanh nhặt đá
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.pickup, 1f);
        }

        Destroy(gameObject);
    }

}
