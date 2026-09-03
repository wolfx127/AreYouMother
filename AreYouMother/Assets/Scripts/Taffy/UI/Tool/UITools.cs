using Taffy.Home;
using Taffy.OverAllManager;
using Taffy.UI;
using UnityEngine;

/// <summary>背包格子的四个方向</summary>
public enum BagDirection
{
    Left,
    Right,
    Up,
    Down,
}

/// <summary>某索引在四个方向上的临近索引（-1 表示无效）</summary>
public struct BagNeighbors
{
    public int left;
    public int right;
    public int up;
    public int down;

    public int Get(BagDirection dir) => dir switch
    {
        BagDirection.Left => left,
        BagDirection.Right => right,
        BagDirection.Up => up,
        _ => down,
    };
}

public static class UITools
{
    private static int GetNeighborIndex(int index, int count, BagDirection dir, int cols = 5)
    {
        int ans = -1;
        if (count <= 0 || index < 0 || index >= count) return -1;
        if (cols <= 0) cols = 5;

        int rows = (count + cols - 1) / cols;
        int row = index / cols;
        int col = index % cols;
        int lastRowWidth = count - (rows - 1) * cols;
        int rowWidth = row == rows - 1 ? lastRowWidth : cols;

        switch (dir)
        {
            case BagDirection.Left:
                ans = col > 0 ? index - 1 : row * cols + rowWidth - 1;
                Debug.Log("向左选");
                break;

            case BagDirection.Right:
                ans = col < rowWidth - 1 ? index + 1 : row * cols;
                Debug.Log("向右选");
                break;

            case BagDirection.Up:
            {
                int targetRow = row == 0 ? rows - 1 : row - 1;
                int targetWidth = targetRow == rows - 1 ? lastRowWidth : cols;
                ans = targetRow * cols + Mathf.Min(col, targetWidth - 1);
                Debug.Log("向上选");
                break;
            }

            case BagDirection.Down:
            {
                int targetRow = row == rows - 1 ? 0 : row + 1;
                int targetWidth = targetRow == rows - 1 ? lastRowWidth : cols;
                ans = targetRow * cols + Mathf.Min(col, targetWidth - 1);
                Debug.Log("向下选");
                break;
            }
        }

        return ans;
    }

    public static BagNeighbors GetNeighbors(int index, int count, int cols = 5) => new BagNeighbors
    {
        left  = GetNeighborIndex(index, count, BagDirection.Left, cols),
        right = GetNeighborIndex(index, count, BagDirection.Right, cols),
        up    = GetNeighborIndex(index, count, BagDirection.Up, cols),
        down  = GetNeighborIndex(index, count, BagDirection.Down, cols),
    };

}
