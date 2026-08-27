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
    public static int GetNeighborIndex(int index, int count, BagDirection dir, int cols = 5)
    {
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
                return col > 0 ? index - 1 : row * cols + rowWidth - 1;

            case BagDirection.Right:
                return col < rowWidth - 1 ? index + 1 : row * cols;

            case BagDirection.Up:
            {
                int targetRow = row == 0 ? rows - 1 : row - 1;
                int targetWidth = targetRow == rows - 1 ? lastRowWidth : cols;
                return targetRow * cols + Mathf.Min(col, targetWidth - 1);
            }

            case BagDirection.Down:
            {
                int targetRow = row == rows - 1 ? 0 : row + 1;
                int targetWidth = targetRow == rows - 1 ? lastRowWidth : cols;
                return targetRow * cols + Mathf.Min(col, targetWidth - 1);
            }
        }
        return -1;
    }

    public static BagNeighbors GetNeighbors(int index, int count, int cols = 5) => new BagNeighbors
    {
        left  = GetNeighborIndex(index, count, BagDirection.Left, cols),
        right = GetNeighborIndex(index, count, BagDirection.Right, cols),
        up    = GetNeighborIndex(index, count, BagDirection.Up, cols),
        down  = GetNeighborIndex(index, count, BagDirection.Down, cols),
    };
}
