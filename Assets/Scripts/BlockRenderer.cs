using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GridLayoutGroup))]
public class BlockRenderer : MonoBehaviour
{
    public GameObject cellPrefab;
    public Color cellColor = Color.cyan;
    public Color emptyColor = new Color(0, 0, 0, 0);
    public Vector2 spacing = Vector2.zero;

    private GridLayoutGroup layout;
    private RectTransform rectTransform;
    private readonly List<GameObject> pool = new List<GameObject>();
    private readonly HashSet<Vector2Int> shapeSet = new HashSet<Vector2Int>();
    private readonly List<GameObject> pendingDisable = new List<GameObject>();

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
        StartCoroutine(RenderDelayed(definition, cellSize));
    }

    private IEnumerator RenderDelayed(BlockDefinition definition, float cellSize)
    {
        yield return null;

        InternalClear();

        if (definition == null || definition.shapeCells == null || definition.shapeCells.Count == 0)
            yield break;

        GetBoundsAndNormalize(definition.shapeCells, out int width, out int height, out var normalized);

        shapeSet.Clear();
        foreach (var c in normalized) shapeSet.Add(c);

        layout.cellSize = new Vector2(cellSize, cellSize);
        layout.spacing = spacing;
        layout.constraintCount = width;

        float totalW = width * cellSize + (width - 1) * spacing.x;
        float totalH = height * cellSize + (height - 1) * spacing.y;
        rectTransform.sizeDelta = new Vector2(totalW, totalH);

        int needed = width * height;
        EnsurePoolSize(needed);

        int index = 0;
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                var go = pool[index++];
                go.SetActive(true);

                var img = go.GetComponent<Image>();
                img.color = shapeSet.Contains(new Vector2Int(x, y)) ? cellColor : emptyColor;
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && pool[i].activeSelf)
                pendingDisable.Add(pool[i]);
        }
        shapeSet.Clear();
    }

    private void InternalClear()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null)
                pool[i].SetActive(false);
        }
        shapeSet.Clear();
    }

    private void LateUpdate()
    {
        if (pendingDisable.Count > 0)
        {
            foreach (var go in pendingDisable)
                if (go != null) go.SetActive(false);

            pendingDisable.Clear();
        }
    }

    private void EnsurePoolSize(int needed)
    {
        while (pool.Count < needed)
        {
            GameObject go = Instantiate(cellPrefab, transform);
            go.SetActive(false);
            pool.Add(go);
        }
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
}
