namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 追击/寻路算法的所有可调参数。
    /// 跳跃高度在这里定死（你游戏里设好的那个定值）。
    /// </summary>
    public static class AlgorithmConfig
    {
        /// <summary>
        /// 跳跃高度定值（世界单位）。
        /// 碰撞箱高度 &gt; 它 = 跳不过去 = A* 当墙绕开；
        /// 高度 &lt;= 它 = 能跳过去 = 不阻挡。
        /// TODO：改成你项目里设好的实际跳跃高度。
        /// </summary>
        public const float JumpHeight = 1.5f;

        /// <summary>A* 格子边长（世界单位），越小路径越贴墙，但搜索越慢。</summary>
        public const float CellSize = 1f;

        /// <summary>单次 A* 最多展开的节点数，超了按“没路”处理，防止卡死。</summary>
        public const int MaxSearchNodes = 4096;

        /// <summary>四叉树重建间隔（秒）。敌人移动快可以调小。</summary>
        public const float QuadtreeRebuildInterval = 0.25f;

        /// <summary>四叉树范围在敌人包围盒基础上往外扩的余量。</summary>
        public const float QuadtreeBoundsMargin = 64f;

        /// <summary>障碍物（PathObstacle 等）重新收集的间隔（秒）。</summary>
        public const float ObstacleRefreshInterval = 1f;

        /// <summary>目标玩家移动超过这个距离就重新寻路。</summary>
        public const float RepathDistance = 0.75f;

        /// <summary>每隔这个时间强制重新寻路一次，防止目标小步移动的误差积累。</summary>
        public const float RepathInterval = 0.5f;

        /// <summary>离路点这个距离内就算到达，切到下一个路点。</summary>
        public const float WaypointArriveDistance = 0.25f;

        /// <summary>离目标玩家这个距离内就不再动了。</summary>
        public const float ArriveRadius = 0.15f;

        /// <summary>
        /// 非 0 时：该 Layer 上的所有非 Trigger 碰撞体都算障碍物；
        /// 0 = 只用挂了 PathObstacle 标记的物体（推荐，不会误伤玩家/敌人自己的碰撞体）。
        /// </summary>
        public static int ObstacleLayerMask = 0;

        /// <summary>是否绘制四叉树调试框（Scene 视图）。</summary>
        public static bool DebugDraw = false;
    }
}
