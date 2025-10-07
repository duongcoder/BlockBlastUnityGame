using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public GridManager gridManager;
    public GridLogic gridLogic;
    public Transform spawnParent;
    public GameObject[] blockPrefabs;
    public Vector2[] localPositions;
    public float gap = 20f;
    public bool refreshOnGridResize = true;
    public bool respawnOnPlaced = true;

    private void OnEnable()
    {
        if (refreshOnGridResize && gridManager != null)
            gridManager.OnGridResized += RefreshAll;
    }

    private void OnDisable()
    {
        if (refreshOnGridResize && gridManager != null)
            gridManager.OnGridResized -= RefreshAll;
    }

    private void Start()
    {
        SpawnAll();
    }

    public void SpawnAll()
    {
        if (spawnParent == null || gridManager == null || blockPrefabs == null || blockPrefabs.Length == 0)
            return;

        float cellSize = gridManager.GetCellSize();

        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            SpawnOne(blockPrefabs[i], i, cellSize);
        }
    }

    private void SpawnOne(GameObject prefab, int index, float cellSize)
    {
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, spawnParent);
        var rt = go.GetComponent<RectTransform>();

        if (localPositions != null && index < localPositions.Length)
        {
            rt.anchoredPosition = localPositions[index];
        }
        else
        {
            rt.anchoredPosition = new Vector2(index * (cellSize * 3f + gap), 0f);
        }

        var drag = go.GetComponent<BlockDrag>();
        if (drag != null)
        {
            drag.gridManager = gridManager;
            drag.gridLogic = gridLogic;

            if (respawnOnPlaced)
            {
                drag.onPlaced += () => OnBlockPlaced(index);
            }
        }

        var renderer = go.GetComponentInChildren<BlockRenderer>();
        var def = drag != null ? drag.definition : null;
        if (renderer != null && def != null)
        {
            renderer.Render(def, cellSize);
        }
    }

    private void RefreshAll()
    {
        if (spawnParent == null || gridManager == null) return;
        float cellSize = gridManager.GetCellSize();

        for (int i = 0; i < spawnParent.childCount; i++)
        {
            var child = spawnParent.GetChild(i);
            var drag = child.GetComponent<BlockDrag>();
            var renderer = child.GetComponentInChildren<BlockRenderer>();
            if (drag != null && renderer != null && drag.definition != null)
            {
                renderer.Render(drag.definition, cellSize);
            }
        }
    }

    private void OnBlockPlaced(int slotIndex)
    {
        if (!respawnOnPlaced) return;
        if (blockPrefabs == null || slotIndex < 0 || slotIndex >= blockPrefabs.Length) return;

        if (slotIndex < spawnParent.childCount)
        {
            var old = spawnParent.GetChild(slotIndex);
            if (old != null) Destroy(old.gameObject);
        }

        float cellSize = gridManager.GetCellSize();
        SpawnOne(blockPrefabs[slotIndex], slotIndex, cellSize);
    }

    public void ClearTray()
    {
        for (int i = spawnParent.childCount - 1; i >= 0; i--)
        {
            Destroy(spawnParent.GetChild(i).gameObject);
        }
    }
}
