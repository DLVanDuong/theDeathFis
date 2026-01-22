using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public class EnemyData : ScriptableObject
{
    [Header("Base Stats")]
    new public string name = "New Enemy";
    public int health = 100; // Sức khỏe của kẻ địch
    public int damage = 10; // Sát thương của kẻ địch
    public float speed = 2f;
    public float runSpeed = 4f;// Tốc độ di chuyển của kẻ địch

    public float attackRange = 2f; // Tầm tấn công
    public float attackCooldown = 1f; // Thời gian hồi chiêu tấn công

    [Header("AI parameters")]
    public float singhtRange = 5f; // Tầm nhìn của kẻ địch
    [Range(0,360)]
    public float singhtAngle = 90f; // Góc nhìn của kẻ địch

    public float chaseRange = 5f; // Tầm đuổi người chơi
    public float losePlayerRange = 10f; // Tầm mất dấu người chơi, quay lại patrol

    [Header("Animation Data")]
    public EnemyAnimationData animationData;
   
}
