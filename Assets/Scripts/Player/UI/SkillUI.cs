using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    // Cấu trúc để quản lý UI của một slot kỹ năng
    [System.Serializable]
    public class SkillSlotUI
    {
        public Image iconImage;          // Biểu tượng kỹ năng
        public Image cooldownOverlay;    // Lớp phủ để hiển thị hồi chiêu
        public TextMeshProUGUI cooldownText; // Text hiển thị thời gian hồi chiêu
    }

    // Các slot kỹ năng trên UI
    public SkillSlotUI skillSlot1;
    public SkillSlotUI skillSlot2;

    public SkillManager skillManager;

    void Update()
    {
        // Cập nhật giao diện mỗi khung hình
        UpdateSlotUI(skillSlot1, skillManager.skill1, skillManager.GetCooldownRemaining(1));
        UpdateSlotUI(skillSlot2, skillManager.skill2, skillManager.GetCooldownRemaining(2));
    }

    private void UpdateSlotUI(SkillSlotUI uiSlot, SkillData skillData, float cooldownRemaining)
    {
        if (skillData != null)
        {
            // Hiển thị biểu tượng và ẩn lớp phủ khi không hồi chiêu
            uiSlot.iconImage.sprite = skillData.skillIcon;
            uiSlot.iconImage.enabled = true;
            uiSlot.cooldownOverlay.fillAmount = cooldownRemaining / skillData.cooldown;
            uiSlot.cooldownText.text = cooldownRemaining > 0 ? cooldownRemaining.ToString("F1") : "";
        }
        else
        {
            // Ẩn tất cả nếu không có kỹ năng nào được gán
            uiSlot.iconImage.enabled = false;
            uiSlot.cooldownOverlay.fillAmount = 0;
            uiSlot.cooldownText.text = "";
        }
    }
}