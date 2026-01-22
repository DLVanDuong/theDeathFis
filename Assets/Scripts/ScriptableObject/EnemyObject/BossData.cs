using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "EnemyData/BossData")]
public class BossData : EnemyData
{
    
    [Header("Boss Abilities // khả năng của boss")]
    public List<BossAbility> abilities; // danh sách các khả năng của boss
    public float abilityCooldown = 8f;// thời gian hồi chiêu giữa các khả năng
    public float minAbilityDistance = 5f; // khoảng cách tối thiểu để sử dụng khả năng
    public float maxAbilityDistance = 15f; // khoảng cách tối đa để sử dụng khả năng

}
