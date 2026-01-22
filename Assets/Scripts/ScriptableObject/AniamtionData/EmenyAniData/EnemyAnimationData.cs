using UnityEngine;

[CreateAssetMenu(fileName = "new EnemyAnimationData", menuName = "Enemy/Animation Data")]
public class EnemyAnimationData : ScriptableObject
{
    [Header("Animation Parameters")]
    public string speedParam = "Speed";
    public string idleIndexParam = "IdleIndex";
    public string playIdleActionTrigger = "PlayIdleAction";
    public string attackTrigger = "Attack";
    public string dieTrigger = "Die";
}
