using UnityEngine;

public enum UpgradeStoneType
{
    Stone_0_5,
    Stone_5_10,
    Stone_Element,
    // Đá nguyên tố
    Stone_Wind,   // Phong
    Stone_Thunder,// Lôi
    Stone_Fire,   // Hỏa
    Stone_Earth   // Thổ
}

[CreateAssetMenu(menuName = "Items/Upgrade Stone Data")]
public class UpgradeStoneData : ScriptableObject
{
    public UpgradeStoneType stoneType;
    public string displayName;
    public Color nameColor = Color.white;
}
