using UnityEngine;

public class CameraFollowTwoPlayers : MonoBehaviour
{
    public Transform player1;                // 玩家1的Transform
    public Transform player2;                // 玩家2的Transform

    [Header("距离限制")]
    public float minPlayerDistance = 2f;     // 两人距离小于此值，摄像机不再拉近
    public float maxPlayerDistance = 15f;    // 两人距离大于此值，摄像机不再拉远

    [Header("摄像机距离范围")]
    public float minCameraDistance = 8f;     // 最近时摄像机离中心的距离
    public float maxCameraDistance = 25f;    // 最远时摄像机离中心的距离

    [Header("固定角度")]
    public Vector3 worldDirection = new Vector3(0f, 1f, -1f); // 摄像机相对世界空间的偏移方向
    // 该向量表示从中心点往上、往后（Z轴负方向）的方向，即典型斜45度俯视

    [Header("平滑速度")]
    public float smoothSpeed = 5f;           // 摄像机移动的平滑度

    private Vector3 currentVelocity;         // 用于 SmoothDamp 的速度参考（这里简单用Lerp）

    void Start()
    {
        if (worldDirection == Vector3.zero)
            worldDirection = new Vector3(0, 1, -1);
        worldDirection.Normalize();           // 确保方向向量长度为1
    }

    void LateUpdate()
    {
        // 如果没有拖入玩家引用，直接返回
        if (player1 == null || player2 == null)
            return;

        // 1. 计算两人中点
        Vector3 center = (player1.position + player2.position) / 2f;

        // 2. 计算两人之间的距离
        float playerDistance = Vector3.Distance(player1.position, player2.position);

        // 3. 根据距离映射摄像机距离 (在min和max之间线性插值)
        float t = Mathf.InverseLerp(minPlayerDistance, maxPlayerDistance, playerDistance);
        float targetCameraDist = Mathf.Lerp(minCameraDistance, maxCameraDistance, t);

        // 4. 计算摄像机的目标位置：中点 + 固定方向 * 目标距离
        Vector3 targetPosition = center + worldDirection * targetCameraDist;

        // 5. 平滑移动摄像机 (使用Lerp，也可以使用Vector3.SmoothDamp)
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 6. 让摄像机始终看向中点
        transform.LookAt(center);
    }
}