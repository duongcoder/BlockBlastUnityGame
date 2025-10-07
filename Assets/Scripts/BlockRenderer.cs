using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(GridLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
public class BlockRenderer : MonoBehaviour
{
    public GameObject cellPrefab;
    public Color cellColor = Color.cyan;
    public Color emptyColor = new Color(0, 0, 0, 0);
    public Vector2 spacing = Vector2.zero;

    private GridLayoutGroup layout;
    private RectTransform rectTransform;
    private readonly List<GameObject> spawnedCells = new List<GameObject>();
    private readonly HashSet<Vector2Int> shapeSet = new HashSet<Vector2Int>();

    private void Awake()
    {
        layout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();

        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    }

    public void Render(BlockDefinition definition, float cellSize)
    {
        Clear();
        if (definition == null || definition.shapeCells == null || definition.shapeCells.Count == 0)
            return;

        GetBoundsAndNormalize(definition.shapeCells, out int width, out int height, out var normalized);

        shapeSet.Clear();
        foreach (var c in normalized) shapeSet.Add(c);

        layout.cellSize = new Vector2(cellSize, cellSize);
        layout.spacing = spacing;
        layout.constraintCount = width;

        float totalW = width * cellSize + (width - 1) * spacing.x;
        float totalH = height * cellSize + (height - 1) * spacing.y;
        rectTransform.sizeDelta = new Vector2(totalW, totalH);

        var parentImage = GetComponent<Image>();
        if (parentImage != null) parentImage.preserveAspect = true;

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject go = Instantiate(cellPrefab, transform);
                var img = go.GetComponent<Image>();
                if (img != null)
                {
                    img.color = shapeSet.Contains(new Vector2Int(x, y)) ? cellColor : emptyColor;
                }
                spawnedCells.Add(go);
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < spawnedCells.Count; i++)
        {
            if (spawnedCells[i] != null)
                Destroy(spawnedCells[i]);
        }
        spawnedCells.Clear();
        shapeSet.Clear();
    }

    private static void GetBoundsAndNormalize(IReadOnlyList<Vector2Int> cells, out int width, out int height, out List<Vector2Int> normalized)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var c in cells)
        {
            if (c.x < minX) minX = c.x;
            if (c.y < minY) minY = c.y;
            if (c.x > maxX) maxX = c.x;
            if (c.y > maxY) maxY = c.y;
        }

        width = maxX - minX + 1;
        height = maxY - minY + 1;

        normalized = new List<Vector2Int>(cells.Count);
        foreach (var c in cells)
        {
            normalized.Add(new Vector2Int(c.x - minX, c.y - minY));
        }
    }

    public Vector2Int GetBlockSize()
    {
        if (layout == null) return Vector2Int.zero;
        return new Vector2Int(layout.constraintCount, Mathf.CeilToInt((float)spawnedCells.Count / layout.constraintCount));
    }
}
