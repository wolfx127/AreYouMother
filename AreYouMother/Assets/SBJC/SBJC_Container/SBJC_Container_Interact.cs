using UnityEngine;
using UnityEngine.InputSystem;

public class SBJC_Container_Interact : MonoBehaviour
{
    [Header("交互按键")]
    public Key player1InteractKey = Key.C;
    public Key player2InteractKey = Key.Numpad2;

    [Header("交互参数")]
    public float interactDistance = 5f;
    public float interactAngle = 180f;
    public float holdDuration = 1.5f;

    private float player1HoldTimer = 0f;
    private float player2HoldTimer = 0f;

    private Transform player1;
    private Transform player2;

    void Start()
    {
        var p1 = GameObject.Find("Player_1");
        if (p1 != null) player1 = p1.transform;
        else Debug.LogError("找不到 Player_1，请检查名称");

        var p2 = GameObject.Find("Player_2");
        if (p2 != null) player2 = p2.transform;
        else Debug.LogError("找不到 Player_2，请检查名称");
    }

    void Update()
    {
        HandlePlayer(player1, player1InteractKey, ref player1HoldTimer, "Player_1");
        HandlePlayer(player2, player2InteractKey, ref player2HoldTimer, "Player_2");
    }

    void HandlePlayer(Transform player, Key interactKey, ref float holdTimer, string playerName)
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        Vector3 toContainer = (transform.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, toContainer);
        bool canInteract = dist < interactDistance && angle < interactAngle;

        if (canInteract && Keyboard.current[interactKey].isPressed)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
            {
                Debug.Log(playerName + " 打开容器");
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }
}
