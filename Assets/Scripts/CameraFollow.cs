using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 0.5f;
    public Vector3 offset;

    public float rotationSpeed = 100f; // 鼠标拖拽旋转速度
    private float currentYaw = 0f;     // 水平旋转角度
    private float currentPitch = 20f;  // 垂直旋转角度
    public float minPitch = -30f;      // 最小垂直旋转角度
    public float maxPitch = 60f;       // 最大垂直旋转角度

    void LateUpdate()
    {
        if (target != null)
        {

            HandleMouseInput();

            Vector3 desiredPosition = target.position + Quaternion.Euler(currentPitch, currentYaw, 0) * offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            transform.LookAt(target.position + Vector3.up * 1.5f); // 视角稍微偏向目标上方
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

            // 限制垂直旋转角度范围
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }
    }
}
