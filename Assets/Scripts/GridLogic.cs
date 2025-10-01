using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class GridLogic : MonoBehaviour
{
    public GridManager gridManager;
    public int width = 8;
    public int height = 8;

    private bool[,] occupied;
    private int score = 0;

    void Awake()
    {
        occupied = new bool[width, height];
    }

    public void OccupyCell(int x, int y, Color color)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            occupied[x, y] = true;
            gridManager.HighlightCell(x, y, color);
        }
    }

    public void CheckAndClearLines()
    {
        List<int> fullRows = new List<int>();
        List<int> fullCols = new List<int>();

        for (int y = 0; y < height; y++)
        {
            bool full = true;
            for (int x = 0; x < width; x++)
            {
                if (!occupied[x, y])
                {
                    full = false;
                    break;
                }
            }
            if (full) fullRows.Add(y);
        }

        for (int x = 0; x < width; x++)
        {
            bool full = true;
            for (int y = 0; y < height; y++)
            {
                if (!occupied[x, y])
                {
                    full = false;
                    break;
                }
            }
            if (full) fullCols.Add(x);
        }

        foreach (int y in fullRows)
        {
            for (int x = 0; x < width; x++)
            {
                occupied[x, y] = false;
                gridManager.HighlightCell(x, y, new Color(1, 1, 1, 0.1f));
            }
            score += 100;
        }

        foreach (int x in fullCols)
        {
            for (int y = 0; y < height; y++)
            {
                occupied[x, y] = false;
                gridManager.HighlightCell(x, y, new Color(1, 1, 1, 0.1f));
            }
            score += 100;
        }

        if (fullRows.Count > 0 || fullCols.Count > 0)
        {
            Debug.Log("Score: " + score);
        }
    }

    public bool IsOccupied(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return true;
        return occupied[x, y];
    }
}
