using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EquipmentSlotButton : MonoBehaviour
{
    public EquipmentManager equipmentManager;
    public EquipmentSlot slot;
    public WeaponDetailUI detailUI;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OpenDetail);
    }

    void OpenDetail()
    {
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);
        if (detailUI == null)
            detailUI = FindFirstObjectByType<WeaponDetailUI>(FindObjectsInactive.Include);

        if (equipmentManager != null && detailUI != null &&
            equipmentManager.TryGetEquippedInstance(slot, out var inst) && inst != null)
        {
            detailUI.Show(inst, WeaponDetailUI.DetailContext.Equipment);
        }
    }
}
