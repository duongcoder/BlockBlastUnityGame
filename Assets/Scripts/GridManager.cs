using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    public GameObject linePrefab;
    public Vector2 paddingRatio = new Vector2(0.03f, 0.03f);
    public float borderThickness = 4f;
    public Color borderColor = Color.yellow;
    public event System.Action OnGridResized;

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
            OnGridResized?.Invoke();
        }
    }

    public float GetBoardBottomLocal(RectTransform parent)
    {
        if (boardRect == null) boardRect = GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        boardRect.GetWorldCorners(corners);
        float worldBottom = corners[0].y;

        Vector3 localBottom = parent.InverseTransformPoint(corners[0]);
        float distanceFromBottom = localBottom.y - parent.rect.yMin;
        return distanceFromBottom;      
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

        float cellSize = Mathf.Min(gridWidth / width, gridHeight / height);

        float totalGridWidth = cellSize * width;
        float totalGridHeight = cellSize * height;
        float offsetX = paddingX + (gridWidth - totalGridWidth) / 2f;
        float offsetY = paddingY + (gridHeight - totalGridHeight) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                RectTransform rt = newCell.GetComponent<RectTransform>();

                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(offsetX + x * cellSize, offsetY + y * cellSize);

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

        float cellSize = Mathf.Min(gridWidth / width, gridHeight / height);

        float totalGridHeight = cellSize * height;
        float totalGridWidth = cellSize * width;
        float offsetX = paddingX + (gridWidth - totalGridWidth) / 2f;
        float offsetY = paddingY + (gridHeight - totalGridHeight) / 2f;

        for (int y = 1; y < height; y++)
        {
            GameObject line = Instantiate(linePrefab, transform);
            RectTransform rt = line.GetComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(totalGridWidth, 2f);
            rt.anchoredPosition = new Vector2(offsetX, offsetY + y * cellSize);
            gridLines.Add(line);
        }

        for (int x = 1; x < width; x++)
        {
            GameObject line = Instantiate(linePrefab, transform);
            RectTransform rt = line.GetComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(2f, totalGridHeight);
            rt.anchoredPosition = new Vector2(offsetX + x * cellSize, offsetY);
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

        float paddingX = boardSize.x * paddingRatio.x;
        float paddingY = boardSize.y * paddingRatio.y;

        float gridWidth = boardSize.x - 2 * paddingX;
        float gridHeight = boardSize.y - 2 * paddingY;

        float cellSize = Mathf.Min(gridWidth / width, gridHeight / height);

        float totalGridWidth = cellSize * width;
        float totalGridHeight = cellSize * height;
        float offsetX = paddingX + (gridWidth - totalGridWidth) / 2f;
        float offsetY = paddingY + (gridHeight - totalGridHeight) / 2f;

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

        MakeBorder(new Vector2(totalGridWidth, borderThickness), new Vector2(offsetX, offsetY));
        MakeBorder(new Vector2(totalGridWidth, borderThickness), new Vector2(offsetX, offsetY + totalGridHeight));
        MakeBorder(new Vector2(borderThickness, totalGridHeight), new Vector2(offsetX, offsetY));
        MakeBorder(new Vector2(borderThickness, totalGridHeight), new Vector2(offsetX + totalGridWidth, offsetY));
    }

    public void HighlightCell(int x, int y, Color color)
    {
        if (cells == null) return;
        if (x < 0 || x >= width || y < 0 || y >= height) return;

        GameObject cell = cells[x, y];
        if (cell == null) return;

        Image img = cell.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    public float GetCellSize()
    {
        if (boardRect == null) boardRect = GetComponent<RectTransform>();

        Vector2 boardSize = boardRect.rect.size;

        float paddingX = boardSize.x * paddingRatio.x;
        float paddingY = boardSize.y * paddingRatio.y;

        float gridWidth = boardSize.x - 2 * paddingX;
        float gridHeight = boardSize.y - 2 * paddingY;

        float cellWidth = gridWidth / width;
        float cellHeight = gridHeight / height;

        return Mathf.Min(cellWidth, cellHeight);
    }

    public RectTransform GetBoardRect()
    {
        if (boardRect == null) boardRect = GetComponent<RectTransform>();
        return boardRect;
    }
}
