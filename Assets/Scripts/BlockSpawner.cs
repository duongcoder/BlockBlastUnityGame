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

    private Dictionary<GameObject, Queue<GameObject>> poolDict;
    private GameObject[] activeBlocks;
    private readonly List<GameObject> pendingReturns = new List<GameObject>();

    private void Awake()
    {
        poolDict = new Dictionary<GameObject, Queue<GameObject>>();
        activeBlocks = new GameObject[slotCount];

        if (blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogWarning("[BlockSpawner] blockPrefabs chưa được gán trong Inspector.");
            blockPrefabs = new GameObject[0];
        }

        foreach (var prefab in blockPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[BlockSpawner] Có prefab bị Missing trong blockPrefabs.");
                continue;
            }
            
            if (!poolDict.ContainsKey(prefab))
                poolDict[prefab] = new Queue<GameObject>();
        }
    }

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

        if (gridManager == null || spawnParent == null) return;

        float cellSize = gridManager.GetCellSize();
        for (int i = 0; i < slotCount; i++)
            SpawnOneAtSlot(i, cellSize);
    }

    private void SpawnOneAtSlot(int slotIndex, float cellSize)
    {
        if (blockPrefabs == null || blockPrefabs.Length == 0) return;

        GameObject prefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];
        if (prefab == null) return;

        GameObject go = GetFromPool(prefab);

        var tag = go.GetComponent<BlockInstance>();
        if (tag == null) tag = go.AddComponent<BlockInstance>();
        tag.prefabRef = prefab;

        go.transform.SetParent(spawnParent, false);
        go.SetActive(true);
        go.transform.SetAsLastSibling();

        var drag = go.GetComponent<BlockDrag>();
        if (drag != null)
        {
            drag.enabled = true;
            drag.gridManager = gridManager;
            drag.gridLogic = gridLogic;
            drag.onPlaced = null;
            drag.rotationSteps = Random.Range(0, 4);

            if (respawnOnPlaced)
            {
                int capturedIndex = slotIndex;
                drag.onPlaced += () => OnBlockPlaced(capturedIndex, go);
            }
        }
        else
        {
            Debug.LogWarning("[BlockSpawner] Prefab thiếu BlockDrag.");
        }

        // Render block
        var renderer = go.GetComponent<BlockRenderer>();
        if (renderer != null && drag != null && drag.definition != null)
        {
            renderer.Render(drag.definition, cellSize, drag.rotationSteps);
        }
        else
        {
            Debug.LogWarning("[BlockSpawner] Prefab missing BlockRenderer component or BlockDefinition.");
        }

        activeBlocks[slotIndex] = go;
    }

    private void OnBlockPlaced(int slotIndex, GameObject instance)
    {
        StartCoroutine(HandlePlacedDelayed(slotIndex, instance));
    }

    private IEnumerator HandlePlacedDelayed(int slotIndex, GameObject instance)
    {
        yield return null;

        if (instance != null)
            pendingReturns.Add(instance);

        float cellSize = (gridManager != null) ? gridManager.GetCellSize() : 0f;
        SpawnOneAtSlot(slotIndex, cellSize);
    }

    private void LateUpdate()
    {
        if (pendingReturns.Count == 0) return;

        foreach (var inst in pendingReturns)
        {
            if (inst == null) continue;

            var tag = inst.GetComponent<BlockInstance>();
            if (tag != null && tag.prefabRef != null)
                ReturnToPool(tag.prefabRef, inst);
            else
                inst.SetActive(false);
        }
        pendingReturns.Clear();
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!poolDict.ContainsKey(prefab))
            poolDict[prefab] = new Queue<GameObject>();

        if (poolDict[prefab].Count > 0)
        {
            return poolDict[prefab].Dequeue();
        }
        else
        {
            var inst = Instantiate(prefab);
            var tag = inst.GetComponent<BlockInstance>();
            if (tag == null) tag = inst.AddComponent<BlockInstance>();
            tag.prefabRef = prefab;
            return inst;
        }
    }

    private void ReturnToPool(GameObject prefab, GameObject instance)
    {
        if (instance == null || prefab == null) return;

        instance.SetActive(false);
        instance.transform.SetParent(transform, false);

        if (!poolDict.ContainsKey(prefab))
            poolDict[prefab] = new Queue<GameObject>();
            
        poolDict[prefab].Enqueue(instance);
    }

    public void ClearTray()
    {
        for (int i = 0; i < activeBlocks.Length; i++)
        {
            var inst = activeBlocks[i];
            if (inst == null) continue;

            var tag = inst.GetComponent<BlockInstance>();
            if (tag != null && tag.prefabRef != null)
                ReturnToPool(tag.prefabRef, inst);
            else
                inst.SetActive(false);

            activeBlocks[i] = null;
        }
    }

    private void RefreshAll()
    {
        if (gridManager == null || spawnParent == null) return;

        float cellSize = gridManager.GetCellSize();
        for (int i = 0; i < spawnParent.childCount; i++)
        {
            var child = spawnParent.GetChild(i);
            var drag = child.GetComponent<BlockDrag>();
            var renderer = child.GetComponent<BlockRenderer>();
            if (drag != null && renderer != null && drag.definition != null)
                renderer.Render(drag.definition, cellSize, drag.rotationSteps);
        }
    }
}
