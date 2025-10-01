using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    public GameObject linePrefab;
    public Vector2 paddingRatio = new Vector2(0.03f, 0.03f);
    public float borderThickness = 4f;
    public Color borderColor = Color.yellow;

    private GameObject[,] cells;
    private RectTransform boardRect;
    private List<GameObject> gridLines = new List<GameObject>();
    private List<GameObject> gridBorder = new List<GameObject>();

    void Start()
    {
        boardRect = GetComponent<RectTransform>();
        GenerateGrid();
        DrawGridLines();
        DrawGridBorder();
    }

    void OnRectTransformDimensionsChange()
    {
        if (boardRect != null && gameObject.activeInHierarchy)
        {
            GenerateGrid();
            DrawGridLines();
            DrawGridBorder();
        }
    }

    void GenerateGrid()
    {
        if (cells != null)
        {
            foreach (var cell in cells)
            {
                if (cell != null) Destroy(cell);
            }
        }

        cells = new GameObject[width, height];

        Vector2 boardSize = boardRect.rect.size;

        float paddingX = boardSize.x * paddingRatio.x;
        float paddingY = boardSize.y * paddingRatio.y;

        float gridWidth = boardSize.x - 2 * paddingX;
        float gridHeight = boardSize.y - 2 * paddingY;

        float cellWidth = gridWidth / width;
        float cellHeight = gridHeight / height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                RectTransform rt = newCell.GetComponent<RectTransform>();

                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
                rt.sizeDelta = new Vector2(cellWidth, cellHeight);
                rt.anchoredPosition = new Vector2(paddingX + x * cellWidth, paddingY + y * cellHeight);

                Image img = newCell.GetComponent<Image>();
                img.color = new Color(1, 1, 1, 0.1f);

                cells[x, y] = newCell;
            }
        }
    }

    void DrawGridLines()
    {
        foreach (var line in gridLines)
        {
            if (line != null) Destroy(line);
        }
        gridLines.Clear();

        Vector2 boardSize = boardRect.rect.size;

        float paddingX = boardSize.x * paddingRatio.x;
        float paddingY = boardSize.y * paddingRatio.y;

        float gridWidth = boardSize.x - 2 * paddingX;
        float gridHeight = boardSize.y - 2 * paddingY;

        float cellWidth = gridWidth / width;
        float cellHeight = gridHeight / height;

        for (int y = 1; y < height; y++)
        {
            GameObject line = Instantiate(linePrefab, transform);
            RectTransform rt = line.GetComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(gridWidth, 2f);
            rt.anchoredPosition = new Vector2(paddingX, paddingY + y * cellHeight);
            gridLines.Add(line);
        }

        for (int x = 1; x < height; x++)
        {
            GameObject line = Instantiate(linePrefab, transform);
            RectTransform rt = line.GetComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(2f, gridHeight);
            rt.anchoredPosition = new Vector2(paddingX + x * cellWidth, paddingY);
            gridLines.Add(line);
        }
    }

    void DrawGridBorder()
    {
        foreach (var b in gridBorder)
        {
            if (b != null) Destroy(b);
        }
        gridBorder.Clear();

        Vector2 boardSize = boardRect.rect.size;

        float paddingX = boardSize.x * 0.05f;
        float paddingY = boardSize.y * 0.05f;

        float gridWidth = boardSize.x - 2 * paddingX;
        float gridHeight = boardSize.y - 2 * paddingY;

        GameObject MakeBorder(Vector2 size, Vector2 pos)
        {
            GameObject line = Instantiate(linePrefab, transform);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var img = line.GetComponent<Image>();
            if (img != null) img.color = borderColor;

            gridBorder.Add(line);
            return line;
        }

        MakeBorder(new Vector2(gridWidth, borderThickness),
                new Vector2(paddingX, paddingY));

        MakeBorder(new Vector2(gridWidth, borderThickness),
                new Vector2(paddingX, paddingY + gridHeight));

        MakeBorder(new Vector2(borderThickness, gridHeight),
                new Vector2(paddingX, paddingY));

        MakeBorder(new Vector2(borderThickness, gridHeight),
                new Vector2(paddingX + gridWidth, paddingY));
    }
    
    public void HighlightCell(int x, int y, Color color)
    {
        if (cells == null) return;
        if (x < 0 || x >= width || y < 0 || y >= height) return;

        GameObject cell = cells[x, y];
        if (cell == null) return;

        Image img = cell.GetComponent<Image>();
        if (img != null)
        {
            img.color = color;
        }
    }
}
