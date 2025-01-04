using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 0.5f;
    public Vector3 offset;

    public float rotationSpeed = 100f; // �����ק��ת�ٶ�
    private float currentYaw = 0f;     // ˮƽ��ת�Ƕ�
    private float currentPitch = 20f;  // ��ֱ��ת�Ƕ�
    public float minPitch = -30f;      // ��С��ֱ��ת�Ƕ�
    public float maxPitch = 60f;       // ���ֱ��ת�Ƕ�

    void LateUpdate()
    {
        if (target != null)
        {

            HandleMouseInput();

            Vector3 desiredPosition = target.position + Quaternion.Euler(currentPitch, currentYaw, 0) * offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            transform.LookAt(target.position + Vector3.up * 1.5f); // �ӽ���΢ƫ��Ŀ���Ϸ�
        }
    }

    void HandleMouseInput()
    {

        if (Input.GetMouseButton(0)) 
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentYaw += mouseX * rotationSpeed * Time.deltaTime;
            currentPitch -= mouseY * rotationSpeed * Time.deltaTime;

            // ���ƴ�ֱ��ת�Ƕȷ�Χ
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }
    }
}
