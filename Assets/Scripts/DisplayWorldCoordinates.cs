using TMPro;
using UnityEngine;

public class DisplayWorldCoordinates : MonoBehaviour
{
    public Transform target; // ��ɫ�� Transform
    private TextMeshProUGUI textMeshPro;

    void Start()
    {
        // ��ȡ TextMeshPro ���
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
            // ��ȡĿ�����������
            Vector3 worldPosition = target.position;

            // ���� TextMeshPro ���ı�����
            textMeshPro.text = $"World Coordinates:\n({worldPosition.x:F2}, {worldPosition.y:F2}, {worldPosition.z:F2})";
        }
    }
}
