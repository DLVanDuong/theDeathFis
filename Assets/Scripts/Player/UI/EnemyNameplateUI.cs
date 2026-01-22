using UnityEngine;
using TMPro;

public class EnemyNameplateUI : MonoBehaviour
{
    public TextMeshProUGUI label;           // kéo Text TMP UGUI vào đây
    public Transform anchor;                // xương đầu/đỉnh model; trống thì dùng root
    public Vector3 worldOffset = new Vector3(0, 2.4f, 0);
    public bool billboard = true;

    private EnemyStateMachine enemy;
    private Camera cam;
    private string lastName;
    private int lastLevel = int.MinValue;

    void Awake()
    {
        enemy = GetComponentInParent<EnemyStateMachine>();
        cam = Camera.main;
        if (!anchor && enemy) anchor = enemy.transform;
    }

    void LateUpdate()
    {
        if (!enemy || enemy.enemyData == null || !label) return;

        // vị trí/định hướng UI
        if (anchor) transform.position = anchor.position + worldOffset;
        if (billboard && cam)
        {
            Vector3 dir = (transform.position - cam.transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        // chỉ cập nhật text khi có thay đổi
        if (enemy.enemyLevel != lastLevel || enemy.enemyData.name != lastName)
        {
            lastLevel = enemy.enemyLevel;
            lastName = enemy.enemyData.name;
            label.text = $"{lastName}  Lv.{lastLevel}";
        }
    }

    // Cho phép EnemyStateMachine gọi cưỡng bức refresh sau khi scale level
    public void RefreshFromEnemy() { lastLevel = int.MinValue; }
}
