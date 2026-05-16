using UnityEngine;

public class SBJC_Container_Interact : MonoBehaviour
{
    [Header("交互按键")]
    public KeyCode player1InteractKey = KeyCode.C;
    public KeyCode player2InteractKey = KeyCode.Keypad2;

    [Header("交互条件")]
    public float interactDistance = 2.5f;      // 多近距离才能交互
    public float interactAngle = 60f;          // 面朝容器的角度阈值
    public float holdDuration = 1.5f;          // 长按时间

    private float player1HoldTimer = 0f;
    private float player2HoldTimer = 0f;

    private Transform player1;
    private Transform player2;

    void Start()
    {
        // 用名字找玩家（请确保 Hierarchy 里名字完全一致）
        GameObject p1 = GameObject.Find("Player_1");
        if (p1 != null) player1 = p1.transform;
        else Debug.LogError("找不到 Player_1，检查名字");

        GameObject p2 = GameObject.Find("Player_2");
        if (p2 != null) player2 = p2.transform;
        else Debug.LogError("找不到 Player_2，检查名字");
    }

    void Update()
    {
        // 处理玩家1
        HandlePlayer(player1, player1InteractKey, ref player1HoldTimer, "Player_1");
        // 处理玩家2
        HandlePlayer(player2, player2InteractKey, ref player2HoldTimer, "Player_2");
    }

    void HandlePlayer(Transform player, KeyCode interactKey, ref float holdTimer, string playerName)
    {
        if (player == null) return;

        // 1. 计算距离
        float dist = Vector3.Distance(transform.position, player.position);
        // 2. 判断是否面朝容器
        Vector3 toContainer = (transform.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, toContainer);
        bool isFacing = angle < interactAngle;
        bool isInRange = dist < interactDistance;

        // 3. 如果满足距离和角度，长按检测
        if (isInRange && isFacing)
        {
            if (Input.GetKey(interactKey))
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdDuration)
                {
                    Debug.Log(playerName + " 打开了容器！（占位）");
                    holdTimer = 0f; // 防止连续触发，可以改成禁用后续触发直到松手
                }
            }
            else
            {
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }
}