using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    
    // A 2D grid to keep track of every individual 1x1 block
    private Transform[,] grid;

    void Awake()
    {
        grid = new Transform[width, height];
    }

    /// <summary>
    /// Call this when a shape is placed onto the board.
    /// </summary>
    public void AddShapeToGrid(Transform parentShape)
    {
        // Loop through all the 1x1 child blocks that make up the shape
        foreach (Transform childBlock in parentShape)
        {
            // Get their exact position on the grid
            int x = Mathf.RoundToInt(childBlock.position.x);
            int y = Mathf.RoundToInt(childBlock.position.y);

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                grid[x, y] = childBlock;
            }
        }
        CheckForFullLinesAndColumns();
    }

    /// <summary>
    /// Checks for full rows and columns simultaneously (standard Block Blast rules).
    /// </summary>
    public int CheckForFullLinesAndColumns()
    {
        List<int> fullRows = new List<int>();
        List<int> fullCols = new List<int>();

        // Check rows
        for (int y = 0; y < height; y++)
        {
            if (HasFullRow(y))
            {
                fullRows.Add(y);
            }
        }

        // Check columns
        for (int x = 0; x < width; x++)
        {
            if (HasFullColumn(x))
            {
                fullCols.Add(x);
            }
        }

        if (fullRows.Count == 0 && fullCols.Count == 0)
        {
            return 0;
        }

        // Collect all unique cells to clear
        HashSet<Vector2Int> cellsToClear = new HashSet<Vector2Int>();
        foreach (int y in fullRows)
        {
            for (int x = 0; x < width; x++)
            {
                cellsToClear.Add(new Vector2Int(x, y));
            }
        }
        foreach (int x in fullCols)
        {
            for (int y = 0; y < height; y++)
            {
                cellsToClear.Add(new Vector2Int(x, y));
            }
        }

        // Clear cells without gravity
        foreach (Vector2Int cell in cellsToClear)
        {
            if (grid[cell.x, cell.y] != null)
            {
                Destroy(grid[cell.x, cell.y].gameObject);
                grid[cell.x, cell.y] = null;
            }
        }

        return fullRows.Count + fullCols.Count;
    }

    bool HasFullRow(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    bool HasFullColumn(int x)
    {
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }
}
