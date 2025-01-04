using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;      // �ƶ��ٶ�
    public float jumpForce = 8f;     // ��Ծ����
    public float gravity = 20f;      // ����ǿ��
    public float slopeLimit = 45f;   // ���½Ƕ�����

    private CharacterController controller; // ��ɫ���������
    private Vector3 moveDirection;          // ��ǰ�ƶ�����
    private bool isGrounded;                // �Ƿ��ڵ���

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
        // ����Ƿ�Ӵ�����
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            // ��ȡ���뷽��
            float moveX = Input.GetAxis("Horizontal"); // A, D
            float moveZ = Input.GetAxis("Vertical");   // W, S

            // �����ƶ����򣨾ֲ�����ϵ��
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            moveDirection = (forward * moveZ + right * moveX).normalized * moveSpeed;

            // ������¿ո�ִ����Ծ
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
            }
        }
        else
        {
            // �ڿ���ʱ����Ӧ������
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // ���¼�⣺�����ƶ���������Ӧ����
        AdjustSlope();

        // Ӧ���ƶ�
        controller.Move(moveDirection * Time.deltaTime);
    }

    void AdjustSlope()
    {
        if (isGrounded)
        {
            RaycastHit hit;

            // ���·������߼������
            if (Physics.Raycast(transform.position, Vector3.down, out hit, controller.height / 2f + 0.1f))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                // �������Ƕ�С������ĽǶ�����
                if (slopeAngle <= slopeLimit)
                {
                    // ���¼����ƶ�����ʹ��ɫ������ƽ��
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                }
                else
                {
                    // �������Ƕȴ������ƣ���ֹǰ��
                    moveDirection.x = 0;
                    moveDirection.z = 0;
                }
            }
        }
    }
}
