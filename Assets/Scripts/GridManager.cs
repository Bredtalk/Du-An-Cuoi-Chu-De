using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    [Tooltip("Number of columns (default: 8 for classic Block Blast)")]
    public int width = 8;

    [Tooltip("Number of rows (default: 8 for classic Block Blast)")]
    public int height = 8;

    [Tooltip("Distance in Unity world units between cell centers")]
    public float cellSize = 1f;

    [Header("Alignment")]
    [Tooltip("If true, the grid will center around this GameObject's position. If false, uses customOrigin.")]
    public bool centerGridOnTransform = true;

    [Tooltip("Used only if centerGridOnTransform is false.")]
    public Vector2 customOrigin = Vector2.zero;

    [Header("Visuals")]
    [Tooltip("Optional prefab for the empty background slot of each cell (1x1 square). If assigned, an 8x8 background grid will be generated automatically at start.")]
    public GameObject cellBackgroundPrefab;

    [Tooltip("Parent transform to hold generated background tiles. If null, this GridManager transform is used.")]
    public Transform backgroundTilesParent;

    [Header("Events & Scoring")]
    public int currentScore = 0;
    public int comboCount = 0;
    public System.Action<int> OnLinesCleared;
    public System.Action OnBlockPlaced;

    // 2D grid storing individual tile transforms
    private Transform[,] grid;

    private void Awake()
    {
        InitializeGrid();
    }

    private void Start()
    {
        if (cellBackgroundPrefab != null)
        {
            GenerateBackgroundGrid();
        }
    }

    public void InitializeGrid()
    {
        grid = new Transform[width, height];
    }

    /// <summary>
    /// Generates visual background cells for the entire grid.
    /// </summary>
    public void GenerateBackgroundGrid()
    {
        Transform parent = backgroundTilesParent != null ? backgroundTilesParent : transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = GetWorldPosition(x, y);
                GameObject cell = Instantiate(cellBackgroundPrefab, pos, Quaternion.identity, parent);
                cell.name = $"Cell_Background_{x}_{y}";
            }
        }
    }

    /// <summary>
    /// Calculates the world coordinate of the bottom-left cell (0, 0).
    /// </summary>
    public Vector2 GetGridOrigin()
    {
        if (centerGridOnTransform)
        {
            float halfWidth = (width - 1) * cellSize * 0.5f;
            float halfHeight = (height - 1) * cellSize * 0.5f;
            return new Vector2(transform.position.x - halfWidth, transform.position.y - halfHeight);
        }
        return customOrigin;
    }

    /// <summary>
    /// Converts grid indices (x, y) to a snapped world position.
    /// </summary>
    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector2 origin = GetGridOrigin();
        return new Vector3(origin.x + x * cellSize, origin.y + y * cellSize, transform.position.z);
    }

    /// <summary>
    /// Converts a world position to nearest grid indices (x, y).
    /// </summary>
    public Vector2Int WorldToGridCoords(Vector3 worldPos)
    {
        Vector2 origin = GetGridOrigin();
        int x = Mathf.RoundToInt((worldPos.x - origin.x) / cellSize);
        int y = Mathf.RoundToInt((worldPos.y - origin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Checks whether (x, y) is within grid bounds.
    /// </summary>
    public bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /// <summary>
    /// Called by DraggableBlock.OnEndDrag() to attempt placing a block.
    /// </summary>
    public bool TryPlaceBlock(DraggableBlock draggedBlock)
    {
        if (grid == null)
        {
            InitializeGrid();
        }

        // 1. Gather all sub-block child transforms that represent 1x1 tiles
        List<Transform> tileTransforms = GetTileChildren(draggedBlock.transform);
        if (tileTransforms.Count == 0)
        {
            return false;
        }

        // 2. Calculate target grid coordinates for each sub-block
        List<Vector2Int> targetCoords = new List<Vector2Int>(tileTransforms.Count);
        HashSet<Vector2Int> uniqueCheck = new HashSet<Vector2Int>();

        foreach (Transform tile in tileTransforms)
        {
            Vector2Int coord = WorldToGridCoords(tile.position);

            // Bounds check
            if (!IsInsideGrid(coord.x, coord.y))
            {
                return false;
            }

            // Occupancy check
            if (grid[coord.x, coord.y] != null)
            {
                return false;
            }

            // Duplicate target check
            if (!uniqueCheck.Add(coord))
            {
                return false;
            }

            targetCoords.Add(coord);
        }

        // 3. ALL sub-blocks valid -> Place and snap onto board
        for (int i = 0; i < tileTransforms.Count; i++)
        {
            Transform tile = tileTransforms[i];
            Vector2Int coord = targetCoords[i];

            // Reparent tile so it remains on the board when draggedBlock is destroyed
            tile.SetParent(transform);
            tile.position = GetWorldPosition(coord.x, coord.y);
            tile.localScale = Vector3.one; // Normalize scale on grid

            // Record in 2D array
            grid[coord.x, coord.y] = tile;
        }

        OnBlockPlaced?.Invoke();

        // 4. Check and clear full rows and columns simultaneously
        CheckForFullLinesAndColumns();

        return true;
    }

    /// <summary>
    /// Checks all rows and columns for full lines, clears them simultaneously,
    /// applies combo bonuses, and damages the enemy boss.
    /// </summary>
    public int CheckForFullLinesAndColumns()
    {
        List<int> fullRows = new List<int>();
        List<int> fullCols = new List<int>();

        // Check rows
        for (int y = 0; y < height; y++)
        {
            bool rowFull = true;
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == null)
                {
                    rowFull = false;
                    break;
                }
            }
            if (rowFull)
            {
                fullRows.Add(y);
            }
        }

        // Check columns
        for (int x = 0; x < width; x++)
        {
            bool colFull = true;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    colFull = false;
                    break;
                }
            }
            if (colFull)
            {
                fullCols.Add(x);
            }
        }

        int totalLinesCleared = fullRows.Count + fullCols.Count;
        if (totalLinesCleared == 0)
        {
            comboCount = 0; // Reset combo if no line cleared on this move
            return 0;
        }

        // Update Combo & Score
        comboCount++;
        int basePoints = totalLinesCleared * 100;
        int comboBonus = (comboCount - 1) * 50;
        currentScore += (basePoints + comboBonus);

        // Collect unique cells to clear (handles intersecting row & column cells safely)
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

        // Destroy tiles and clear references
        foreach (Vector2Int cell in cellsToClear)
        {
            if (grid[cell.x, cell.y] != null)
            {
                Destroy(grid[cell.x, cell.y].gameObject);
                grid[cell.x, cell.y] = null;
            }
        }

        // Notify listeners (UI score updates, sound effects, etc.)
        OnLinesCleared?.Invoke(totalLinesCleared);

        // Damage the Enemy boss with combo multiplier
        Enemy boss = FindFirstObjectByType<Enemy>();
        if (boss != null)
        {
            boss.TakeDamage();
            if (boss.health != null)
            {
                int damage = totalLinesCleared * 10 + (comboCount - 1) * 5;
                boss.health.TakeDamage(damage);
            }
        }

        return totalLinesCleared;
    }

    /// <summary>
    /// Helper to find all 1x1 sub-blocks under a shape parent.
    /// </summary>
    private List<Transform> GetTileChildren(Transform parent)
    {
        List<Transform> tiles = new List<Transform>();
        foreach (Transform child in parent)
        {
            if (child.GetComponent<SpriteRenderer>() != null || 
                child.GetComponent<UnityEngine.UI.Image>() != null || 
                child.childCount == 0)
            {
                tiles.Add(child);
            }
        }

        // If parent itself has no children, treat parent as single 1x1 block
        if (tiles.Count == 0)
        {
            tiles.Add(parent);
        }

        return tiles;
    }

    /// <summary>
    /// Visualizes the grid cells in the Unity Scene View for easy alignment.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        Vector2 origin = GetGridOrigin();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 center = new Vector3(origin.x + x * cellSize, origin.y + y * cellSize, transform.position.z);
                Gizmos.DrawWireCube(center, new Vector3(cellSize * 0.95f, cellSize * 0.95f, 0.05f));
            }
        }
    }
}