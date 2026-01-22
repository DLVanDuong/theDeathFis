using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform contentParent;
    public GameObject buttonPrefab;
    [SerializeField] private WeaponDetailUI detailUI;
    public static InventoryUI Instance { get; private set; }

    void Awake()
    {
        if (detailUI == null)
            detailUI = FindFirstObjectByType<WeaponDetailUI>(FindObjectsInactive.Include);
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        var equipMgr = FindFirstObjectByType<EquipmentManager>();
        if (equipMgr != null)
            equipMgr.EquipmentChanged += RefreshUI;

        if (Inventory.Instance != null)
            Inventory.Instance.OnChanged += RefreshUI;

        RefreshUI();

        if (ZoneInfoUI.Instance != null)
            ZoneInfoUI.Instance.Suppress(true);
    }

    void OnDisable()
    {
        var equipMgr = FindFirstObjectByType<EquipmentManager>();
        if (equipMgr != null)
            equipMgr.EquipmentChanged -= RefreshUI;

        if (Inventory.Instance != null)
            Inventory.Instance.OnChanged -= RefreshUI;

        if (ZoneInfoUI.Instance != null)
            ZoneInfoUI.Instance.UnsuppressAndRestore();
    }

    public void RefreshUI()
    {
        if (Inventory.Instance == null) return;

        // clear hết cũ
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // build lại từ list hiện tại
        foreach (var inst in Inventory.Instance.weapons)
        {
            var btnObj = Instantiate(buttonPrefab, contentParent);
            btnObj.name = inst.template.weaponName;

            var icon = btnObj.transform.Find("Icon")?.GetComponent<UnityEngine.UI.Image>();
            var txt = btnObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

            if (icon) icon.sprite = inst.template.icon;
            if (txt)
            {
                txt.text = RarityDisplay.FormatDisplayName(inst.template.weaponName, inst.rarity, inst.upgradeLevel);
                txt.color = RarityDisplay.GetRarityColor(inst.rarity);
            }

            var instLocal = inst;
            var btn = btnObj.GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() =>
            {
                if (detailUI == null)
                    detailUI = FindFirstObjectByType<WeaponDetailUI>(FindObjectsInactive.Include);

                detailUI?.Show(instLocal, WeaponDetailUI.DetailContext.Bag);
            });
        }

       
    }
}
