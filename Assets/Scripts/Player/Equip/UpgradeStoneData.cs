using UnityEngine;

public enum UpgradeStoneType
{
    Stone_0_5,
    Stone_5_10
}

[CreateAssetMenu(menuName = "Items/Upgrade Stone Data")]
public class UpgradeStoneData : ScriptableObject
{
    public UpgradeStoneType stoneType;
    public string displayName;
    public Color nameColor = Color.white;
}
