using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;

    private GameObject[,] cells;
    private RectTransform boardRect;

    void Start()
    {
        boardRect = GetComponent<RectTransform>();
        GenerateGrid();

        ColorCell(0, 0, Color.red);
    }

    void GenerateGrid()
    {
        cells = new GameObject[width, height];

        // Lấy kích thước thực tế của GameBoard sau khi Canvas scale
        Vector2 boardSize = boardRect.rect.size;

        float cellWidth = boardSize.x / width;
        float cellHeight = boardSize.y / height;

        // Tính toán Offset
        Vector2 gridOriginRatio = new Vector2(0.02f, 0.02f);
        Vector2 gridOrigin = new Vector2(boardSize.x * gridOriginRatio.x, boardSize.y * gridOriginRatio.y);


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                RectTransform rt = newCell.GetComponent<RectTransform>();

                // Đặt anchor/pivot để bám vào góc dưới trái của GameBoard
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);

                // Đặt kích thước và vị trí từng ô
                rt.sizeDelta = new Vector2(cellWidth, cellHeight);
                rt.anchoredPosition = new Vector2(x * cellWidth, y * cellHeight) + gridOrigin;

                cells[x, y] = newCell;
            }
        }

        // for (int x = 0; x < width; x++)
        // {
        //     for (int y = 0; y < height; y++)
        //     {
        //         GameObject newCell = Instantiate(cellPrefab, transform);
        //         cells[x, y] = newCell;
        //     }
        // }

        // float cellWidth = boardRect.rect.width / width;
        // float cellHeight = boardRect.rect.height / height;

        // for (int x = 0; x < width; x++)
        // {
        //     for (int y = 0; y < height; y++)
        //     {
        //         GameObject newCell = Instantiate(cellPrefab, transform);
        //         RectTransform rt = newCell.GetComponent<RectTransform>();

        //         rt.anchorMin = new Vector2(0, 0);
        //         rt.anchorMax = new Vector2(0, 0);
        //         rt.pivot = new Vector2(0, 0);

        //         rt.sizeDelta = new Vector2(cellWidth, cellHeight);
        //         rt.anchoredPosition = new Vector2(x * cellWidth, y * cellHeight);

        //         cells[x, y] = newCell;
        //     }
        // }
    }

    public void ColorCell(int x, int y, Color color)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            Image img = cells[x, y].GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
            }
        }
    }
}
