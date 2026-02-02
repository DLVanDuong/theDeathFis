using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUIController : MonoBehaviour
{
    const float BASE_SUCCESS = 0.10f;
    const float FAIL_BONUS_ADD = 0.05f;
    const float MAX_SUCCESS = 0.90f;

    [Header("Root")]
    public GameObject panelRoot;

    [Header("Buttons")]
    public Button btnCheTao;

    [Header("Result Slot (UI)")]
    public Image resultSlotIcon;

    [Header("Database")]
    public WeaponDatabase weaponDatabase;

    [Header("UI Slots")]
    public Image weaponSlotIcon;   // ô 1
    public Image elementSlotIcon;  // ô 2
    public Image stoneSlotIcon;    // ô 3 (phôi đá)

    [Header("Slot Buttons (click)")]
    public Button slot1WeaponBtn;
    public Button slot2ElementBtn;
    public Button slot3CatalystBtn;

    [Header("Picker")]
    public CraftPickerUI picker;

    [Header("Catalyst (Slot 3: Phôi đá/Đá ngũ sắc)")]
    public Sprite elementCatalystIcon;
    public int catalystNeed = 1;
    public int elementStoneNeed = 1;

    [Header("Counts UI (tuỳ chọn)")]
    public TextMeshProUGUI catalystCountText;
    public TextMeshProUGUI elementCountText;

    [Header("Element Sprites (Slot 2)")]
    public Sprite windIcon, thunderIcon, fireIcon, earthIcon;

    public WeaponInstance selectedWeapon;
    public UpgradeStoneType selectedElementStone = UpgradeStoneType.Stone_Fire;
    private bool hasCatalystSelected;

    void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;

        // ✅ Auto find picker (kể cả inactive)
        if (picker == null) picker = GetComponentInChildren<CraftPickerUI>(true);

        // ✅ Auto find slot buttons theo path đúng như Hierarchy bạn chụp
        if (slot1WeaponBtn == null)
            slot1WeaponBtn = transform.Find("Slots_LeftGrid/slot_1")?.GetComponent<Button>();

        if (slot2ElementBtn == null)
            slot2ElementBtn = transform.Find("Slots_LeftGrid/slot_2")?.GetComponent<Button>();

        if (slot3CatalystBtn == null)
            slot3CatalystBtn = transform.Find("Slots_LeftGrid/slot_3")?.GetComponent<Button>();

        Debug.Log($"[CraftUI] slot1={(slot1WeaponBtn ? slot1WeaponBtn.name : "NULL")} " +
                  $"slot2={(slot2ElementBtn ? slot2ElementBtn.name : "NULL")} " +
                  $"slot3={(slot3CatalystBtn ? slot3CatalystBtn.name : "NULL")} " +
                  $"picker={(picker ? picker.name : "NULL")} panelRoot={(panelRoot ? panelRoot.name : "NULL")}");

        if (btnCheTao) btnCheTao.onClick.AddListener(OnCraftClicked);

        if (slot1WeaponBtn)
        {
            slot1WeaponBtn.onClick.RemoveAllListeners();
            slot1WeaponBtn.onClick.AddListener(OnClickSlotWeapon);
        }
        if (slot2ElementBtn)
        {
            slot2ElementBtn.onClick.RemoveAllListeners();
            slot2ElementBtn.onClick.AddListener(OnClickSlotElement);
        }
        if (slot3CatalystBtn)
        {
            slot3CatalystBtn.onClick.RemoveAllListeners();
            slot3CatalystBtn.onClick.AddListener(OnClickSlotCatalyst);
        }

        // Bạn có thể tắt panel tại đây nếu muốn
        // panelRoot.SetActive(false);

        RefreshSlotsUI();
    }

    public void Toggle()
    {
        if (!panelRoot) return;

        panelRoot.SetActive(!panelRoot.activeSelf);

        if (panelRoot.activeSelf)
        {
            panelRoot.transform.SetAsLastSibling(); // 🔥 đè tất cả UI khác
            RefreshSlotsUI();
        }
    }

    List<WeaponInstance> Inv_GetWeapons()
    {
        if (Inventory.Instance == null) return new List<WeaponInstance>();
        return Inventory.Instance.weapons;
    }

    int Inv_GetStoneCount(UpgradeStoneType type)
    {
        if (Inventory.Instance == null) return 0;
        return Inventory.Instance.GetStoneCount(type);
    }

    bool Inv_ConsumeCatalyst(int amount)
    {
        return Inventory.Instance != null &&
               Inventory.Instance.ConsumeUpgradeStone(UpgradeStoneType.Stone_Element, amount);
    }

    public void OnClickSlotWeapon()
    {
        Debug.Log("[CraftUI] Click slot_1");

        if (picker == null)
        {
            Debug.LogError("[CraftUI] picker == NULL");
            return;
        }

        var weapons = Inv_GetWeapons();
        Debug.Log($"[CraftUI] weapons.Count = {(weapons != null ? weapons.Count : -1)}");

        if (weapons == null || weapons.Count == 0)
        {
            Debug.LogWarning("[CraftUI] Không có vũ khí trong túi (Inventory.Instance.weapons rỗng).");
            return;
        }

        picker.Open(
            weapons,
            w => (w != null && w.template != null) ? w.template.icon : null,
            w => (w != null && w.template != null) ? w.template.weaponName : "Weapon",
            w => SetSelectedWeapon(w)
        );
    }


    public void OnClickSlotElement()
    {
        Debug.Log("[CraftUI] Click slot_2");

        if (picker == null)
        {
            Debug.LogError("[CraftUI] picker == NULL.");
            return;
        }

        var elements = new List<UpgradeStoneType>
        {
            UpgradeStoneType.Stone_Wind,
            UpgradeStoneType.Stone_Thunder,
            UpgradeStoneType.Stone_Fire,
            UpgradeStoneType.Stone_Earth
        };

        picker.Open(
            elements,
            e => GetElementSprite(e),
            e => $"{GetElementName(e)} x{Inv_GetStoneCount(e)}",
            e => SetSelectedElement(e)
        );
    }

    public void OnClickSlotCatalyst()
    {
        int have = Inv_GetStoneCount(UpgradeStoneType.Stone_Element);

        if (!hasCatalystSelected)
        {
            if (have < catalystNeed)
            {
                Debug.LogWarning($"[Craft] Thiếu phôi đá (Stone_Element). Cần {catalystNeed}, hiện có {have}.");
                return;
            }
            hasCatalystSelected = true;
        }
        else
        {
            hasCatalystSelected = false;
        }

        RefreshSlotsUI();
    }

    public void SetSelectedWeapon(WeaponInstance w)
    {
        selectedWeapon = w;

        if (!weaponSlotIcon) return;

        if (w == null)
        {
            weaponSlotIcon.sprite = null;
            weaponSlotIcon.enabled = false;
            return;
        }

        Sprite s = (w.template != null) ? w.template.icon : null;

        weaponSlotIcon.sprite = s;
        weaponSlotIcon.enabled = (s != null);

        if (resultSlotIcon)
        {
            resultSlotIcon.sprite = s;
            resultSlotIcon.enabled = (s != null);
        }
    }

    public void SetSelectedElement(UpgradeStoneType elementType)
    {
        selectedElementStone = elementType;

        if (!elementSlotIcon) return;
        elementSlotIcon.sprite = GetElementSprite(elementType);
        elementSlotIcon.enabled = (elementSlotIcon.sprite != null);

        RefreshSlotsUI();
    }

    void RefreshSlotsUI()
    {
        if (weaponSlotIcon)
            weaponSlotIcon.enabled = (weaponSlotIcon.sprite != null);

        if (elementSlotIcon)
        {
            elementSlotIcon.sprite = GetElementSprite(selectedElementStone);
            elementSlotIcon.enabled = (elementSlotIcon.sprite != null);
        }

        if (stoneSlotIcon)
        {
            stoneSlotIcon.sprite = hasCatalystSelected ? elementCatalystIcon : null;
            stoneSlotIcon.enabled = hasCatalystSelected && elementCatalystIcon != null;
        }

        if (catalystCountText)
            catalystCountText.text = Inv_GetStoneCount(UpgradeStoneType.Stone_Element).ToString();

        if (elementCountText)
            elementCountText.text = Inv_GetStoneCount(selectedElementStone).ToString();
    }

    void OnCraftClicked()
    {
        if (selectedWeapon == null) { Debug.LogWarning("[Craft] Chưa chọn vũ khí (ô 1)."); return; }
        if (!IsElementStone(selectedElementStone)) { Debug.LogWarning("[Craft] Chưa chọn nguyên tố (ô 2)."); return; }
        if (!hasCatalystSelected) { Debug.LogWarning("[Craft] Chưa đặt phôi đá (ô 3)."); return; }

        if (Inventory.Instance == null || !Inventory.Instance.ConsumeUpgradeStone(selectedElementStone, elementStoneNeed))
        {
            Debug.LogWarning($"[Craft] Thiếu đá nguyên tố {GetElementName(selectedElementStone)} x{elementStoneNeed}!");
            return;
        }

        if (!Inv_ConsumeCatalyst(catalystNeed))
        {
            Inventory.Instance?.AddUpgradeStone(selectedElementStone, elementStoneNeed);
            Debug.LogWarning("[Craft] Thiếu phôi đá (Stone_Element)!");
            return;
        }

        float chance = Mathf.Clamp(BASE_SUCCESS + selectedWeapon.elementCraftBonus, 0f, MAX_SUCCESS);
        float roll = Random.value;

        if (roll <= chance)
        {
            selectedWeapon.hasElementStone = true;
            selectedWeapon.elementStone = selectedElementStone;
            selectedWeapon.elementCraftBonus = 0f;

            var equipMgr = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);
            equipMgr?.RefreshEquippedWeaponVFX();

            Debug.Log($"[Craft] THÀNH CÔNG! ({chance:P0}) => {selectedElementStone}");
        }
        else
        {
            selectedWeapon.elementCraftBonus += FAIL_BONUS_ADD;
            Debug.Log($"[Craft] THẤT BẠI! +{FAIL_BONUS_ADD:P0} lần sau. ({chance:P0})");
        }

        hasCatalystSelected = false;
        RefreshSlotsUI();
    }

    bool IsElementStone(UpgradeStoneType t)
    {
        return t == UpgradeStoneType.Stone_Wind
            || t == UpgradeStoneType.Stone_Thunder
            || t == UpgradeStoneType.Stone_Fire
            || t == UpgradeStoneType.Stone_Earth;
    }

    Sprite GetElementSprite(UpgradeStoneType t) => t switch
    {
        UpgradeStoneType.Stone_Wind => windIcon,
        UpgradeStoneType.Stone_Thunder => thunderIcon,
        UpgradeStoneType.Stone_Fire => fireIcon,
        UpgradeStoneType.Stone_Earth => earthIcon,
        _ => null
    };

    string GetElementName(UpgradeStoneType e) => e switch
    {
        UpgradeStoneType.Stone_Wind => "Phong",
        UpgradeStoneType.Stone_Thunder => "Lôi",
        UpgradeStoneType.Stone_Fire => "Hỏa",
        UpgradeStoneType.Stone_Earth => "Thổ",
        _ => "?"
    };
}
