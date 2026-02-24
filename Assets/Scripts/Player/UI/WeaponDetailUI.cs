using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponDetailUI : MonoBehaviour
{
    public enum DetailContext { Bag, Equipment }

    [Header("Refs")]
    public GameObject panelRoot;        // Panel_WeaponDetail
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public Button equipButton;
    public Button unequipButton;
    public Button closeButton;
    public TextMeshProUGUI hintText;    // (optional)
    public GameObject panelEquipment;   // (optional) Panel_Equipment

    [Header("Upgrade")]
    public Button upgradeButton;
    public TextMeshProUGUI upgradeCostText;

    [Header("Upgrade Stone UI")]
    public Image stoneIcon;                 // Image icon đá (UI)
    public TextMeshProUGUI stoneCountText;  // TMP số đá (UI)
    public Sprite stone0to5Sprite;          // icon đá 0->5
    public Sprite stone5to10Sprite;         // icon đá 5->10

    [Header("Element Stone (Inlaid) UI")]
    public Image elementStoneIcon;          // icon nguyên tố đã khảm
    public Sprite windIcon, thunderIcon, fireIcon, earthIcon;

    [Header("Sell (Bag Only)")]
    public Button sellButton;           // KÉO nút Sell vào đây
    public TextMeshProUGUI sellPriceText; // KÉO text giá bán vào đây

    public static bool IsOpen { get; private set; }

    private WeaponInstance current;
    private EquipmentManager equipMgr;

    void Awake()
    {
        equipMgr = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);

        if (closeButton)
            closeButton.onClick.AddListener(() =>
            {
                if (panelRoot) panelRoot.SetActive(false);
                IsOpen = false;
            });

        if (panelRoot)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
            IsOpen = true;
        }
    }

    void Start()
    {
        // đảm bảo nằm trên cùng và nhận raycast
        if (panelRoot != null)
        {
            var c = panelRoot.GetComponent<Canvas>();
            if (c == null) c = panelRoot.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 500;

            if (panelRoot.GetComponent<GraphicRaycaster>() == null)
                panelRoot.AddComponent<GraphicRaycaster>();

            panelRoot.transform.SetAsLastSibling();
        }

        if (icon) icon.raycastTarget = false;
        if (nameText) nameText.raycastTarget = false;
        if (statsText) statsText.raycastTarget = false;

        if (panelRoot != null)
        {
            var cg = panelRoot.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelRoot.AddComponent<CanvasGroup>();

            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.ignoreParentGroups = true;
        }
    }

    public void Show(WeaponInstance inst, DetailContext ctx = DetailContext.Bag)
    {
        current = inst;
        if (current == null) return;

        // --- Điền UI cơ bản ---
        if (nameText)
        {
            string rarityName = RarityDisplay.FormatDisplayName(current.template.weaponName, current.rarity, current.upgradeLevel);
            nameText.text = rarityName;
            nameText.color = RarityDisplay.GetRarityColor(current.rarity);
        }

        if (icon) icon.sprite = current.template.icon;

        if (statsText)
        {
            statsText.text =
$@"Tấn Công: {current.damage}
Sức Mạnh: {current.strength}
Nhanh Nhẹn: {current.agility}
Thể Lực: {current.vitality}
Năng Lượng: {current.energy}
Yêu cầu Lv: {current.requiredLevel}";
        }

        // --- Tính điều kiện ---
        var targetSlot = TargetSlotOf(current); // Bow/Shield -> LeftHand, còn lại RightHand
        EquipmentSlot eqSlot = default;

        bool isThisEquipped = (equipMgr != null) && equipMgr.IsEquipped(current, out eqSlot);
        bool slotOccupied = (equipMgr != null) && equipMgr.HasItemInSlot(targetSlot);

        int playerLv = 1;
        var levelSys = FindFirstObjectByType<PlayerLevelSystem>();
        if (levelSys != null && levelSys.playerStats != null)
            playerLv = levelSys.playerStats.level;

        int reqLv = current.requiredLevel;
        bool enoughLevel = playerLv >= reqLv;

        // reset listener mỗi lần mở
        equipButton?.onClick.RemoveAllListeners();
        unequipButton?.onClick.RemoveAllListeners();
        sellButton?.onClick.RemoveAllListeners();
        upgradeButton?.onClick.RemoveAllListeners();

        // bật panel
        if (panelRoot) { panelRoot.SetActive(true); panelRoot.transform.SetAsLastSibling(); }
        if (panelEquipment) panelEquipment.SetActive(true);

        // ====== CONTEXT: BAG ======
        if (ctx == DetailContext.Bag)
        {
            // EQUIP
            if (equipButton)
            {
                equipButton.gameObject.SetActive(true);
                equipButton.interactable = true;

                equipButton.onClick.AddListener(() =>
                {
                    if (!enoughLevel)
                    {
                        if (hintText) hintText.text = $"<color=#FF6666>Cần cấp {reqLv} để trang bị</color>";
                        return;
                    }
                    if (slotOccupied)
                    {
                        if (hintText) hintText.text = "Slot này đang có trang bị, hãy tháo ra trước!";
                        return;
                    }
                    if (isThisEquipped)
                    {
                        if (hintText) hintText.text = "Bạn đang đeo món này rồi!";
                        return;
                    }

                    if (equipMgr != null && equipMgr.TryEquipIfSlotFree(targetSlot, current))
                    {
                        InventoryUI.Instance?.RefreshUI();
                        panelRoot?.SetActive(false);
                        IsOpen = false;

                    }
                    else
                    {
                        if (hintText) hintText.text = "Không thể trang bị (slot bận / state bị khóa).";
                    }
                });
            }

            if (unequipButton) unequipButton.gameObject.SetActive(false);

            // SELL PRICE + SELL BUTTON
            int sellPrice = WeaponSellCalculator.GetSellPrice(current);
            if (sellPriceText) sellPriceText.text = $"Bán: {sellPrice} coin";

            if (sellButton)
            {
                bool canSell = ShopTrigger.IsPlayerNearShop;

                sellButton.gameObject.SetActive(true);
                sellButton.interactable = canSell;

                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(() =>
                {
                    if (!ShopTrigger.IsPlayerNearShop)
                    {
                        if (hintText) hintText.text = "<color=#FF6666>Chỉ bán được khi đứng gần Shop</color>";
                        return;
                    }

                    if (Inventory.Instance == null)
                    {
                        Debug.LogError("[Sell] Inventory.Instance = null");
                        return;
                    }

                    if (PlayerWallet.Instance == null)
                    {
                        Debug.LogError("[Sell] PlayerWallet.Instance = null");
                        return;
                    }

                    int price = WeaponSellCalculator.GetSellPrice(current);

                    // + coin
                    PlayerWallet.Instance.AddCoin(price);

                    // remove khỏi túi
                    Inventory.Instance.RemoveWeapon(current);

                    InventoryUI.Instance?.RefreshUI();
                    panelRoot?.SetActive(false); 
                    IsOpen = false;
                });
            }

            if (upgradeButton != null)
            {
                bool canUp = current.CanUpgrade();
                upgradeButton.gameObject.SetActive(true);

                if (!canUp)
                {
                    upgradeButton.interactable = false;
                    if (upgradeCostText) upgradeCostText.text = "Max (+10)";
                    if (stoneIcon) stoneIcon.enabled = false;
                    if (stoneCountText) stoneCountText.text = "";
                }
                else
                {
                    // +0 -> +5 dùng đá 0->5 | +5 -> +10 dùng đá 5->10
                    UpgradeStoneType needStone =
                        (current.upgradeLevel < 5) ? UpgradeStoneType.Stone_0_5 : UpgradeStoneType.Stone_5_10;

                    int have = (Inventory.Instance != null) ? Inventory.Instance.GetStoneCount(needStone) : 0;
                    bool enough = have >= 1;

                    upgradeButton.interactable = enough;

                    if (upgradeCostText)
                        upgradeCostText.text = (needStone == UpgradeStoneType.Stone_0_5)
                            ? $"Update: cần 1 Đá (0→5) (đang có {have})"
                            : $"Update: cần 1 Đá (5→10) (đang có {have})";

                    if (stoneIcon)
                    {
                        stoneIcon.enabled = true;
                        stoneIcon.sprite = (needStone == UpgradeStoneType.Stone_0_5) ? stone0to5Sprite : stone5to10Sprite;
                    }

                    if (stoneCountText)
                        stoneCountText.text = $"x{have}";

                    upgradeButton.onClick.RemoveAllListeners();
                    upgradeButton.onClick.AddListener(() =>
                    {
                        if (Inventory.Instance == null) return;

                        if (!current.CanUpgrade())
                        {
                            if (hintText) hintText.text = "<color=#FF6666>Đã đạt cấp tối đa</color>";
                            return;
                        }

                        UpgradeStoneType need =
                            (current.upgradeLevel < 5) ? UpgradeStoneType.Stone_0_5 : UpgradeStoneType.Stone_5_10;

                        if (!Inventory.Instance.ConsumeUpgradeStone(need, 1))
                        {
                            if (hintText)
                            {
                                hintText.text = (need == UpgradeStoneType.Stone_0_5)
                                    ? "<color=#FF6666>Thiếu Đá Cường Hóa (0→5)</color>"
                                    : "<color=#FF6666>Thiếu Đá Cường Hóa (5→10)</color>";
                            }
                            return;
                        }
                        current.CaptureRolledBase();

                        // ✅ rồi mới nâng cấp
                        current.UpgradeOnce();

                        equipMgr?.RefreshEquippedWeaponVFX();

                        InventoryUI.Instance?.RefreshUI();
                        Show(current, DetailContext.Bag);
                    });
                }
            }

            // HINT
            if (hintText)
            {
                if (!enoughLevel) hintText.text = $"<color=#FF6666>Không đủ cấp (cần Lv {reqLv})</color>";
                else if (sellButton && !sellButton.interactable) hintText.text = "<color=#FF6666>Hãy đứng gần Shop để bán</color>";
                else hintText.text = "";
            }

            return;
        }

        // ====== CONTEXT: EQUIPMENT ======
        if (ctx == DetailContext.Equipment)
        {
            if (unequipButton)
            {
                unequipButton.gameObject.SetActive(true);
                unequipButton.interactable = true;

                var slotToUnequip = isThisEquipped ? eqSlot : targetSlot;
                unequipButton.onClick.AddListener(() =>
                {
                    if (equipMgr != null)
                    {
                        equipMgr.UnequipSlot(slotToUnequip); // tháo và trả về túi
                        InventoryUI.Instance?.RefreshUI();
                        panelRoot?.SetActive(false);
                        IsOpen = false;
                    }
                });
            }

            if (equipButton) equipButton.gameObject.SetActive(false);

            // ẨN SELL khi đang xem đồ đang mặc
            if (sellButton) sellButton.gameObject.SetActive(false);
            if (sellPriceText) sellPriceText.text = "";

            if (hintText) hintText.text = "";
        }
        RefreshInlaidElementUI();
    }

    private EquipmentSlot TargetSlotOf(WeaponInstance inst)
    {
        int type = inst.template.weaponTypeID;

        // Bow (3) hoặc Shield (9) → LeftHand
        if (type == 3 || type == 9)
            return EquipmentSlot.LeftHand;

        if (type == 8)
            return EquipmentSlot.RightHand;

        if (inst.template.slot == EquipmentSlot.LeftHand)
            return EquipmentSlot.LeftHand;

        return EquipmentSlot.RightHand;
    }
    private Sprite GetElementStoneSprite(UpgradeStoneType t)
    {
        switch (t)
        {
            case UpgradeStoneType.Stone_Wind: return windIcon;
            case UpgradeStoneType.Stone_Thunder: return thunderIcon;
            case UpgradeStoneType.Stone_Fire: return fireIcon;
            case UpgradeStoneType.Stone_Earth: return earthIcon;
            default: return null;
        }
    }

    private void RefreshInlaidElementUI()
    {
        if (!elementStoneIcon) return;

        bool show = (current != null && current.hasElementStone);
        if (!show)
        {
            elementStoneIcon.enabled = false;   // ✅ ẨN khi chưa khảm
            elementStoneIcon.sprite = null;
            return;
        }

        elementStoneIcon.sprite = GetElementStoneSprite(current.elementStone);
        elementStoneIcon.enabled = (elementStoneIcon.sprite != null); // ✅ CHỈ HIỆN khi có sprite
    }
}
