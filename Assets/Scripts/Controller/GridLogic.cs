using UnityEngine;
using System.Collections.Generic;

public struct BlockInfo
{
    public BlockDefinition def;
    public int rotationSteps;

    public BlockInfo(BlockDefinition def, int rotationSteps)
    {
        this.def = def;
        this.rotationSteps = rotationSteps;
    }
}

public class GridLogic : MonoBehaviour
{
    public System.Action<int> OnScoreChanged;
    public System.Action OnGameOver;

    [SerializeField] private GridManager gridManager;
    [SerializeField] private int pointsPerLine = 100;
    private bool[,] occupied;
    private int score = 0;

    void Awake()
    {
        InitGrid();
    }

    private void InitGrid()
    {
        if (gridManager == null) return;
        occupied = new bool[gridManager.width, gridManager.height];
    }

    public void ResetGrid()
    {
        InitGrid();
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                occupied[x, y] = false;
                gridManager.HighlightCell(x, y, new Color(1, 1, 1, 0.1f));
            }
        }
        score = 0;
        OnScoreChanged?.Invoke(score);
    }

    public void OccupyCell(int x, int y, Color color)
    {
        if (InBounds(x, y))
        {
            occupied[x, y] = true;
            gridManager.HighlightCell(x, y, color);
        }
    }

    public void ClearCell(int x, int y)
    {
        if (InBounds(x, y))
        {
            occupied[x, y] = false;
            gridManager.HighlightCell(x, y, new Color(1, 1, 1, 0.1f));
        }
    }

    public bool IsOccupied(int x, int y)
    {
        if (!InBounds(x, y)) return true;
        return occupied[x, y];
    }

    public void CheckAndClearLines()
    {
        List<int> fullRows = GetFullRows();
        List<int> fullCols = GetFullCols();

        int clearedLines = 0;

        foreach (int y in fullRows)
        {
            for (int x = 0; x < gridManager.width; x++)
            {
                ClearCell(x, y);
            }
            clearedLines++;
        }

        foreach (int x in fullCols)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                ClearCell(x, y);
            }
            clearedLines++;
        }

        if (clearedLines > 0)
        {
            AddScore(clearedLines * pointsPerLine);
        }
    }

    private void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public int GetScore()
    {
        return score;
    }

    private List<int> GetFullRows()
    {
        List<int> fullRows = new List<int>();
        for (int y = 0; y < gridManager.height; y++)
        {
            bool full = true;
            for (int x = 0; x < gridManager.width; x++)
            {
                if (!occupied[x, y]) { full = false; break; }
            }
            if (full) fullRows.Add(y);
        }
        return fullRows;
    }

    private List<int> GetFullCols()
    {
        List<int> fullCols = new List<int>();
        for (int x = 0; x < gridManager.width; x++)
        {
            bool full = true;
            for (int y = 0; y < gridManager.height; y++)
            {
                if (!occupied[x, y]) { full = false; break; }
            }
            if (full) fullCols.Add(x);
        }
        return fullCols;
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < gridManager.width && y >= 0 && y < gridManager.height;
    }

    public void CheckGameOver(List<BlockInfo> trayBlocks)
    {
        Debug.Log("[GridLogic] CheckGameOver called");
        if (!CanPlaceAnyBlock(trayBlocks))
        {
            Debug.Log("[GridLogic] No block can be placed → GAME OVER");
            OnGameOver?.Invoke();
        }
        else
        {
            Debug.Log("[GridLogic] Still at least one block can be placed");
        }
    }

    private bool CanPlaceAnyBlock(List<BlockInfo> trayBlocks)
    {
        foreach (var info in trayBlocks)
        {
            bool can = (info.def != null && CanPlaceBlock(info.def, info.rotationSteps));
            Debug.Log($"[GridLogic] Can place {info.def?.name} rot={info.rotationSteps}? {can}");
            if (can) return true;
        }
        return false;
    }

    private bool CanPlaceBlock(BlockDefinition def, int rotationSteps)
    {
        int maxX = gridManager.width - def.GetWidth(rotationSteps);
        int maxY = gridManager.height - def.GetHeight(rotationSteps);

        for (int x = 0; x <= maxX; x++)
        {
            for (int y = 0; y <= maxY; y++)
            {
                if (CanPlaceAt(def, x, y, rotationSteps))
                {
                    Debug.Log($"[GridLogic] {def.name} rot={rotationSteps} CAN place at ({x},{y})");
                    return true;
                }
            }
        }
        return false;
    }

    private bool CanPlaceAt(BlockDefinition def, int startX, int startY, int rotationSteps)
    {
        foreach (Vector2Int cell in def.GetNormalizedCells(rotationSteps))
        {
            int gridX = startX + cell.x;
            int gridY = startY + cell.y;

            if (!InBounds(gridX, gridY)) return false;
            if (occupied[gridX, gridY]) return false;
        }
        Debug.Log($"[GridLogic] {def.name} rot={rotationSteps} CAN place at ({startX},{startY})");
        return true;
    }
}
