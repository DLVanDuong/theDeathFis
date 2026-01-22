using UnityEngine;

[CreateAssetMenu(fileName = "Character/Thông Tin", menuName = "Scriptable Objects/Character")]
public class Character : ScriptableObject
{   
    public float maxHealth = 100f;
    public float maxMana = 100f;
    public int attack = 10;
    public int defense = 5;
    public float speed = 4f;
    public float RunSpeed = 6f;
    public float atackRange = 2f;

    [Header("Player Stats")]
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;
    public int statPoints = 0;

    [Header("Attributes")]
    public int strength = 5;  // Tăng sức tấn công
    public int agility = 5;   // Tăng tốc độ đánh
    public int vitality = 5;  // Tăng máu
    public int energy = 5;    // Tăng mana
}
