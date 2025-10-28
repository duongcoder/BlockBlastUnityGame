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
    private readonly List<GameObject> gridLines = new List<GameObject>();
    private readonly List<GameObject> gridBorder = new List<GameObject>();

    void Start()
    {
        boardRect = GetComponent<RectTransform>();
        RegenerateGrid();
    }

    void OnRectTransformDimensionsChange()
    {
        if (boardRect != null && gameObject.activeInHierarchy)
        {
            RegenerateGrid();
            OnGridResized?.Invoke();
        }
    }

    private void RegenerateGrid()
    {
        ClearGrid();
        GenerateGrid();
        DrawGridLines();
        DrawGridBorder();
    }

    private void ClearGrid()
    {
        if (cells != null)
        {
            foreach (var cell in cells)
                if (cell != null) Destroy(cell);
        }
        foreach (var line in gridLines)
            if (line != null) Destroy(line);
        foreach (var b in gridBorder)
            if (b != null) Destroy(b);

        gridLines.Clear();
        gridBorder.Clear();
    }

    private void GenerateGrid()
    {
        cells = new GameObject[width, height];

        float cellSize = GetCellSize();
        float offsetX, offsetY;
        GetOffsets(cellSize, out offsetX, out offsetY);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                RectTransform rt = newCell.GetComponent<RectTransform>();

                rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(offsetX + (x + 0.5f) * cellSize, offsetY + (y + 0.5f) * cellSize);

                Image img = newCell.GetComponent<Image>();
                if (img != null) img.color = new Color(1, 1, 1, 0.1f);

                cells[x, y] = newCell;
            }
        }
    }

    private void DrawGridLines()
    {
        float cellSize = GetCellSize();
        float offsetX, offsetY;
        GetOffsets(cellSize, out offsetX, out offsetY);

        float totalGridWidth = cellSize * width;
        float totalGridHeight = cellSize * height;

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

    private void DrawGridBorder()
    {
        float cellSize = GetCellSize();
        float offsetX, offsetY;
        GetOffsets(cellSize, out offsetX, out offsetY);

        float totalGridWidth = cellSize * width;
        float totalGridHeight = cellSize * height;

        void MakeBorder(Vector2 size, Vector2 pos)
        {
            GameObject line = Instantiate(linePrefab, transform);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var img = line.GetComponent<Image>();
            if (img != null) img.color = borderColor;

            gridBorder.Add(line);
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

        int pixelSize = Mathf.FloorToInt(Mathf.Min(cellWidth, cellHeight));

        return Mathf.Max(1, pixelSize);
    }

    private void GetOffsets(float cellSize, out float offsetX, out float offsetY)
    {
        Vector2 boardSize = boardRect.rect.size;

        float paddingX = boardSize.x * paddingRatio.x;
        float paddingY = boardSize.y * paddingRatio.y;

        float gridWidth = boardSize.x - 2 * paddingX;
        float gridHeight = boardSize.y - 2 * paddingY;

        float totalGridWidth = cellSize * width;
        float totalGridHeight = cellSize * height;

        offsetX = paddingX + (gridWidth - totalGridWidth) / 2f;
        offsetY = paddingY + (gridHeight - totalGridHeight) / 2f;
    }

    public Vector2 GetGridOffset()
    {
        float cellSize = GetCellSize();
        float offsetX, offsetY;
        GetOffsets(cellSize, out offsetX, out offsetY);
        return new Vector2(offsetX, offsetY);
    }

    public float GetBoardBottom(RectTransform parent)
    {
        if (boardRect == null) boardRect = GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        boardRect.GetWorldCorners(corners);
        Vector3 localBottom = parent.InverseTransformPoint(corners[0]);
        return localBottom.y - parent.rect.yMin;
    }

    public RectTransform GetBoardRect()
    {
        if (boardRect == null) boardRect = GetComponent<RectTransform>();
        return boardRect;
    }

    public float GetBoardBottomLocal(RectTransform parent)
    {
        return GetBoardBottom(parent);
    }

    public float GetBoardTopLocal(RectTransform parent)
    {
        if (boardRect == null) boardRect = GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        boardRect.GetWorldCorners(corners);

        Vector3 localTop = parent.InverseTransformPoint(corners[1]);

        return parent.rect.yMax - localTop.y;
    }
}
