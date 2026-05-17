using UnityEngine;
using UnityEngine.InputSystem;

public class SBJC_Player_2_Movement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -9.81f;

    private CharacterController PlayerController;
    private Vector3 velocity;

    void Start()
    {
        PlayerController = GetComponent<CharacterController>();
        velocity = Vector3.zero;
    }

    void Update()
    {
        if (PlayerController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = 0f;
        float moveZ = 0f;
        if (Keyboard.current.upArrowKey.isPressed)    moveZ += 1f;
        if (Keyboard.current.downArrowKey.isPressed)  moveZ -= 1f;
        if (Keyboard.current.leftArrowKey.isPressed)  moveX -= 1f;
        if (Keyboard.current.rightArrowKey.isPressed) moveX += 1f;

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;
        Vector3 horizontalMove = move * speed * Time.deltaTime;

        velocity.y += gravity * Time.deltaTime;

        Vector3 fullMove = horizontalMove;
        fullMove.y = velocity.y * Time.deltaTime;

        PlayerController.Move(fullMove);

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}