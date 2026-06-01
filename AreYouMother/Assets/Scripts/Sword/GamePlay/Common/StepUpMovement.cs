using UnityEngine;

/// <summary>
/// 自动攀爬矮障碍物的静态工具类。
/// 原理：前方BoxCast检测 → 障碍物高度判断 → 头顶空间检测 → 自动抬升Y轴。
/// BoxCast 半尺寸与角色 BoxCollider 一致，形状完全匹配。
/// </summary>
public static class StepUpMovement
{
    /// <summary>
    /// 执行带自动攀爬的移动，返回最终应设置的 transform.position。
    /// 如果前方有矮障碍物，自动抬高Y轴翻越；如果障碍物太高或被挡住，则原地不动。
    /// </summary>
    /// <param name="currentPos">角色当前位置 (transform.position)</param>
    /// <param name="moveAmount">本帧移动向量（World Space，通常已含 deltaTime）</param>
    /// <param name="halfExtents">角色BoxCollider的半尺寸 (size * 0.5)</param>
    /// <param name="maxStepHeight">能翻越的最大高度</param>
    /// <param name="obstacleLayer">障碍物所在Layer的mask</param>
    /// <returns>最终应设置给 transform.position 的位置</returns>
    public static Vector3 MoveWithStepUp(
        Vector3 currentPos,
        Vector3 moveAmount,
        Vector3 halfExtents,
        float maxStepHeight = 0.5f,
        LayerMask obstacleLayer = default)
    {
        if (obstacleLayer == default)
            obstacleLayer = ~0;

        Vector3 targetPos = currentPos + moveAmount;

        // 水平移动量为零时不检测
        Vector3 horizontalMove = new Vector3(moveAmount.x, 0, moveAmount.z);
        if (horizontalMove.sqrMagnitude < 0.0001f)
            return targetPos;

        Vector3 moveDir = horizontalMove.normalized;
        float moveDist = horizontalMove.magnitude;
        const float skinWidth = 0.01f;

        // ===== 第1步：从角色正前方做 BoxCast，形状与角色一致 =====
        // 起点向前推一个半尺寸，确保Box完全在自身Collider之外
        float horizontalExtent = Mathf.Max(halfExtents.x, halfExtents.z);
        float forwardOffset = horizontalExtent + skinWidth;
        Vector3 castOrigin = currentPos + moveDir * forwardOffset;
        // 不旋转Box，对齐世界轴（角色是方的，障碍物也是方的）
        Quaternion orientation = Quaternion.identity;

        if (!Physics.BoxCast(castOrigin, halfExtents, moveDir, out RaycastHit hit,
            orientation, moveDist, obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            // 前方无障碍，正常移动
            return targetPos;
        }

        // ===== 第2步：判断障碍物高度是否可翻越 =====
        float obstacleTop = hit.collider.bounds.max.y;
        float stepHeight = obstacleTop - currentPos.y;

        // 太高或比角色还低 → 阻挡
        if (stepHeight <= 0f || stepHeight > maxStepHeight)
            return currentPos;

        // ===== 第3步：检查翻越后头顶是否有足够空间 =====
        // 在攀爬高度再做一次 BoxCast
        float elevatedY = currentPos.y + stepHeight + halfExtents.y + skinWidth;
        Vector3 elevatedOrigin = new Vector3(currentPos.x, elevatedY, currentPos.z);

        // 检测距离需覆盖：前置偏移 + 到障碍物距离 + 半尺寸，确保完整站上去
        float headroomCheckDist = forwardOffset + hit.distance + horizontalExtent;
        if (Physics.BoxCast(elevatedOrigin, halfExtents, moveDir, out _,
            orientation, headroomCheckDist, obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            // 头顶有遮挡
            return currentPos;
        }

        // ===== 第4步：翻越！ =====
        targetPos.y = obstacleTop;
        return targetPos;
    }
}
