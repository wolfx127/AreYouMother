using UnityEngine;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 寻路障碍标记：挂在“敌人跳不过去”的碰撞箱上。
    ///
    /// 判定规则：
    ///   碰撞箱世界高度 &gt; AlgorithmConfig.JumpHeight → A* 当墙绕开；
    ///   高度 &lt;= JumpHeight → 能跳过去，不阻挡（路径可以直接穿过，穿的时候走跳跃逻辑）。
    ///
    /// 没挂 Collider 的物体可以手动填 size 指定包围盒。
    /// </summary>
    public class PathObstacle : MonoBehaviour
    {
        [Tooltip("没有 Collider 时用的手动包围盒大小（世界单位）")]
        public Vector3 size = Vector3.zero;

        Collider col;

        void Awake()
        {
            col = GetComponent<Collider>();
        }

        /// <summary>世界 xy 平面上的包围矩形。</summary>
        public Rect GetWorldRect()
        {
            if (col != null)
            {
                Bounds b = col.bounds;
                return new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
            }

            Vector3 s = size.sqrMagnitude > 0.001f ? size : Vector3.one;
            Vector3 p = transform.position;
            return new Rect(p.x - s.x * 0.5f, p.y - s.y * 0.5f, s.x, s.y);
        }

        /// <summary>世界高度（y 方向尺寸），用来和跳跃高度比较。</summary>
        public float GetWorldHeight()
        {
            if (col != null) return col.bounds.size.y;
            return size.sqrMagnitude > 0.001f ? size.y : 1f;
        }
    }
}
