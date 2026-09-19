using UnityEngine;

namespace Taffy.Play
{
    /// <summary>
    /// 双人相机（最终方案）：
    /// - 朝向固定：yaw 与俯角在 Awake 捕获，之后永不动 → 屏幕方向 = 世界方向，WASD 零换算
    /// - 水平位置：平滑跟随两玩家中点（左右/前后平移都在这）
    /// - 高度：按两玩家在相机本地横/纵轴上的分离分量换算，保证两人始终在屏幕内（带边距）
    /// 玩法配套：给两玩家设最大距离（限距），画面不会拉太远。
    /// </summary>
    public class TwoPlayerCamera : MonoBehaviour
    {
        [Header("两个玩家（Inspector 里拖角色）")]
        [SerializeField] private Transform playerA;
        [SerializeField] private Transform playerB;

        [Header("玩家与屏幕两侧保持的世界距离")]
        [SerializeField] private float sideMargin = 1f;

        [Header("最低高度（两人贴脸时的基准构图）")]
        [SerializeField] private float minHeight = 8f;

        [Header("平滑速度（越大跟得越紧）")]
        [SerializeField] private float followSpeed = 5f;

        [Header("兜底校正：每次后撤距离与最大步数")]
        [SerializeField] private float dollyStep = 0.5f;
        [SerializeField] private int maxIterations = 40;

        private Camera cam;
        private Vector3 backFlat;   // 视线在地面的水平后退方向（Awake 捕获，永不变）
        private float tanPitch;     // 俯角正切（Awake 捕获，永不变）
        private float currentDist;  // 当前后撤距离（唯一状态量，平滑它）

        private void Awake()
        {
            cam = GetComponent<Camera>();

            // 俯角固定：从 Inspector 里摆好的初始朝向捕获，之后永不动
            float pitchX = transform.eulerAngles.x;
            tanPitch = Mathf.Tan(pitchX * Mathf.Deg2Rad);

            Vector3 flat = new Vector3(-transform.forward.x, 0f, -transform.forward.z);
            if (flat.sqrMagnitude < 1e-6f) flat = Vector3.back;   // 垂直下视退化
            backFlat = flat.normalized;

            // 初始后撤距离 = 最低高度换算
            currentDist = minHeight / tanPitch;
            transform.position = OnRay(Mid());
        }

        private void LateUpdate()
        {
            if (!playerA || !playerB || !cam) return;

            // 两玩家水平连线（忽略高度差），投到相机本地轴
            Vector3 sep = playerB.position - playerA.position;
            sep.y = 0f;
            Vector3 localSep = cam.transform.InverseTransformDirection(sep);
            float needX = Mathf.Abs(localSep.x) / 2f + sideMargin;   // 横轴半宽需求
            float needZ = Mathf.Abs(localSep.z) / 2f + sideMargin;   // 纵轴半高需求

            // 固定 yaw 没有横轴优化：纵轴是最坏情况，两轴各自换算取大者
            float tanHalfV = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float needHeight = Mathf.Max(needZ / tanHalfV, needX / (tanHalfV * cam.aspect));
            float targetDist = Mathf.Max(minHeight / tanPitch, needHeight / tanPitch);

            // 平滑后撤距离，相机始终在穿过中点的视线上 → 中心不漂移
            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            currentDist = Mathf.Lerp(currentDist, targetDist, t);
            transform.position = OnRay(Mid());

            // 兜底：俯角小/地形不平导致公式差一点时，立刻后撤到两人都进屏幕（不平滑）
            for (int i = 0; i < maxIterations; i++)
            {
                if (BothInView()) break;
                currentDist += dollyStep;
                transform.position = OnRay(Mid());
            }

            // 朝向从不改：屏幕方向 = 世界方向，WASD 直接映射，永不混乱
        }

        // 穿过中点的视线：位置只由"后撤距离"决定（水平后退 + 按俯角抬高）
        private Vector3 OnRay(Vector3 mid)
            => mid + backFlat * currentDist + Vector3.up * (currentDist * tanPitch);

        private Vector3 Mid()
        {
            return (playerA.position + playerB.position) / 2f;
        }

        private bool BothInView()
        {
            return InBand(playerA) && InBand(playerB);
        }

        private bool InBand(Transform target)
        {
            Vector3 vp = cam.WorldToViewportPoint(target.position);
            return vp.z > 0f
                && vp.x > 0.05f && vp.x < 0.95f
                && vp.y > 0.05f && vp.y < 0.95f;
        }
    }
}
