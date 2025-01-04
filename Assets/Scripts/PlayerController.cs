using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;      // 移动速度
    public float jumpForce = 8f;     // 跳跃力度
    public float gravity = 20f;      // 重力强度
    public float slopeLimit = 45f;   // 爬坡角度限制

    private CharacterController controller; // 角色控制器组件
    private Vector3 moveDirection;          // 当前移动方向
    private bool isGrounded;                // 是否在地面

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // 检测是否接触地面
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            // 获取输入方向
            float moveX = Input.GetAxis("Horizontal"); // A, D
            float moveZ = Input.GetAxis("Vertical");   // W, S

            // 设置移动方向（局部坐标系）
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            moveDirection = (forward * moveZ + right * moveX).normalized * moveSpeed;

            // 如果按下空格，执行跳跃
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
            }
        }
        else
        {
            // 在空中时，仅应用重力
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // 爬坡检测：调整移动方向以适应坡面
        AdjustSlope();

        // 应用移动
        controller.Move(moveDirection * Time.deltaTime);
    }

    void AdjustSlope()
    {
        if (isGrounded)
        {
            RaycastHit hit;

            // 向下发射射线检测坡面
            if (Physics.Raycast(transform.position, Vector3.down, out hit, controller.height / 2f + 0.1f))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                // 如果坡面角度小于允许的角度限制
                if (slopeAngle <= slopeLimit)
                {
                    // 重新计算移动方向，使角色与坡面平行
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                }
                else
                {
                    // 如果坡面角度大于限制，禁止前进
                    moveDirection.x = 0;
                    moveDirection.z = 0;
                }
            }
        }
    }
}
