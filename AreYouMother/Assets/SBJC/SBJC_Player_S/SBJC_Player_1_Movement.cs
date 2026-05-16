using UnityEngine;

public class SBJC_Player_1_Movement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -9.81f;

    private CharacterController PlayerController;
    private Vector3 velocity;           // 累积垂直速度的成员变量（重要！）

    void Start()
    {
        PlayerController = GetComponent<CharacterController>();
        velocity = Vector3.zero;
    }

    void Update()
    {
        // 着地处理
        if (PlayerController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 保持贴地，防止浮空
        }

        // 水平输入
        float moveX = 0f;
        float moveZ = 0f;
        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;
        Vector3 horizontalMove = move * speed * Time.deltaTime;

        // 重力加速度累积
        velocity.y += gravity * Time.deltaTime;

        // 组合水平与垂直移动
        Vector3 fullMove = horizontalMove;
        fullMove.y = velocity.y * Time.deltaTime;

        // 执行移动
        PlayerController.Move(fullMove);

        // 转向（保留）
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}