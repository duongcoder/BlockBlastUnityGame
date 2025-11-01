using UnityEngine;
using System.Collections;
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
    public System.Action<int, int> OnLinesCleared;
    public System.Action OnGameOver;

    [SerializeField] private GridManager gridManager;
    [SerializeField] private int pointsPerLine = 100;
    [SerializeField] private Color flashColor = new Color(1f, 1f, 0.2f, 0.9f);
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Color emptyColor = new Color(1, 1, 1, 0.1f);
    private bool[,] occupied;
    private int score = 0;
    private int comboCount = 0;

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
        comboCount = 0;
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
            gridManager.HighlightCell(x, y, emptyColor);
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

        int cleared = 0;

        foreach (int y in fullRows)
        {
            StartCoroutine(FlashRow(y));
            cleared++;
        }

        foreach (int x in fullCols)
        {
            StartCoroutine(FlashCol(x));
            cleared++;
        }

        if (cleared > 0)
        {
            comboCount++;
            int points = cleared * pointsPerLine * comboCount;
            AddScore(points);

            Debug.Log($"[GridLogic] Cleared {cleared} lines/cols, combo x{comboCount}, + {points} điểm");
            OnLinesCleared?.Invoke(cleared, comboCount);
        }
        else
        {
            comboCount = 0;
        }
    }

    private IEnumerator FlashRow(int y)
    {
        if (gridManager == null) yield break;

        for (int x = 0; x < gridManager.width; x++)
            gridManager.HighlightCell(x, y, flashColor);

        yield return new WaitForSeconds(flashDuration);

        for (int x = 0; x < gridManager.width; x++)
            ClearCell(x, y);
    }

    private IEnumerator FlashCol(int x)
    {
        if (gridManager == null) yield break;

        for (int y = 0; y < gridManager.height; y++)
            gridManager.HighlightCell(x, y, flashColor);

        yield return new WaitForSeconds(flashDuration);

        for (int y = 0; y < gridManager.height; y++)
            ClearCell(x, y);
    }

    private void AddScore(int amount)
    {
        if (amount <= 0) return;
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public int GetScore()
    {
        return score;
    }

    private List<int> GetFullRows()
    {
        var fullRows = new List<int>();
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
        var fullCols = new List<int>();
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
        if (!CanPlaceAnyBlock(trayBlocks))
        {
            Debug.Log("[GridLogic] No block can be placed → GAME OVER");
            OnGameOver?.Invoke();
        }
    }

    private bool CanPlaceAnyBlock(List<BlockInfo> trayBlocks)
    {
        foreach (var info in trayBlocks)
        {
            if (info.def != null && CanPlaceBlock(info.def, info.rotationSteps))
                return true;
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
                    return true;
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
        return true;
    }
}
