using UnityEngine;
using UnityEngine.InputSystem;

public class SBJC_Player_1_Movement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -9.81f;

    private CharacterController PlayerController;
    private Vector3 velocity;           // �ۻ���ֱ�ٶȵĳ�Ա��������Ҫ����

    void Start()
    {
        PlayerController = GetComponent<CharacterController>();
        velocity = Vector3.zero;
    }

    void Update()
    {
        // �ŵش���
        if (PlayerController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // �������أ���ֹ����
        }

        // ˮƽ����
        float moveX = 0f;
        float moveZ = 0f;
        if (Keyboard.current.wKey.isPressed) moveZ += 1f;
        if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.dKey.isPressed) moveX += 1f;

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;
        Vector3 horizontalMove = move * speed * Time.deltaTime;

        // �������ٶ��ۻ�
        velocity.y += gravity * Time.deltaTime;

        // ���ˮƽ�봹ֱ�ƶ�
        Vector3 fullMove = horizontalMove;
        fullMove.y = velocity.y * Time.deltaTime;

        // ִ���ƶ�
        PlayerController.Move(fullMove);

        // ת�򣨱�����
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}