using TMPro;
using UnityEngine;

public class DisplayWorldCoordinates : MonoBehaviour
{
    public Transform target; // 角色的 Transform
    private TextMeshProUGUI textMeshPro;

    void Start()
    {
        // 获取 TextMeshPro 组件
        textMeshPro = GetComponent<TextMeshProUGUI>();

        if (target == null)
        {
            Debug.Log("Need player");
        }
    }

    void Update()
    {
        if (target != null && textMeshPro != null)
        {
            // 获取目标的世界坐标
            Vector3 worldPosition = target.position;

            // 更新 TextMeshPro 的文本内容
            textMeshPro.text = $"World Coordinates:\n({worldPosition.x:F2}, {worldPosition.y:F2}, {worldPosition.z:F2})";
        }
    }
}
