using UnityEngine;
using System.Collections.Generic;

public class GridLogic : MonoBehaviour
{
    public GridManager gridManager;
    public System.Action<int> OnScoreChanged;

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

        foreach (int y in fullRows)
        {
            for (int x = 0; x < gridManager.width; x++)
            {
                ClearCell(x, y);
            }
            score += 100;
        }

        foreach (int x in fullCols)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                ClearCell(x, y);
            }
            score += 100;
        }

        if (fullRows.Count > 0 || fullCols.Count > 0)
        {
            Debug.Log("Score: " + score);
            OnScoreChanged?.Invoke(score);
        }
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
}
