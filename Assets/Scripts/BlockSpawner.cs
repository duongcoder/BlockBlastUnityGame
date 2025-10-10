using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockSpawner : MonoBehaviour
{
    public GridManager gridManager;
    public GridLogic gridLogic;
    public Transform spawnParent;
    public GameObject[] blockPrefabs;
    public bool refreshOnGridResize = true;
    public bool respawnOnPlaced = true;
    public int slotCount = 3;

    private Dictionary<GameObject, Queue<GameObject>> poolDict = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();
    private GameObject[] activeBlocks;

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

    private void Awake()
    {
        activeBlocks = new GameObject[slotCount];
        foreach (var prefab in blockPrefabs)
        {
            poolDict[prefab] = new Queue<GameObject>();
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnDelayed());
    }

    private IEnumerator SpawnDelayed()
    {
        yield return null;
        SpawnAll();
    }

    public void SpawnAll()
    {
        ClearTray();

        float cellSize = gridManager.GetCellSize();

        for (int i = 0; i < slotCount; i++)
        {
            SpawnOneAtSlot(i, cellSize);
        }
    }

    private void SpawnOneAtSlot(int slotIndex, float cellSize)
    {
        if (blockPrefabs.Length == 0) return;

        GameObject prefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];
        GameObject go = GetFromPool(prefab);
        go.transform.SetParent(spawnParent, false);
        go.SetActive(true);

        var drag = go.GetComponent<BlockDrag>();
        if (drag != null)
        {
            drag.gridManager = gridManager;
            drag.gridLogic = gridLogic;
            drag.onPlaced = null;

            if (respawnOnPlaced)
            {
                int capturedIndex = slotIndex;
                drag.onPlaced += () => OnBlockPlaced(capturedIndex, go);
            }
        }

        var renderer = go.GetComponentInChildren<BlockRenderer>();
        if (renderer != null && drag != null && drag.definition != null)
        {
            renderer.Render(drag.definition, cellSize);
        }

        activeBlocks[slotIndex] = go;
        instanceToPrefab[go] = prefab;
    }

    private void OnBlockPlaced(int slotIndex, GameObject instance)
    {
        StartCoroutine(HandlePlacedDelayed(slotIndex, instance));
    }

    private IEnumerator HandlePlacedDelayed(int slotIndex, GameObject instance)
    {
        yield return null;

        if (instance != null && instanceToPrefab.TryGetValue(instance, out var prefab))
        {
            ReturnToPool(prefab, instance);
            instanceToPrefab.Remove(instance);
        }

        float cellSize = gridManager.GetCellSize();
        SpawnOneAtSlot(slotIndex, cellSize);
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (poolDict[prefab].Count > 0)
        {
            return poolDict[prefab].Dequeue();
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    private void ReturnToPool(GameObject prefab, GameObject instance)
    {
        if (instance == null) return;
        instance.SetActive(false);
        instance.transform.SetParent(transform, false);
        poolDict[prefab].Enqueue(instance);
    }

    public void ClearTray()
    {
        for (int i = 0; i < activeBlocks.Length; i++)
        {
            if (activeBlocks[i] != null)
            {
                var inst = activeBlocks[i];
                if (instanceToPrefab.TryGetValue(inst, out var prefab))
                {
                    ReturnToPool(prefab, inst);
                    instanceToPrefab.Remove(inst);
                }
                activeBlocks[i] = null;
            }
        }
    }

    private void RefreshAll()
    {
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
}
