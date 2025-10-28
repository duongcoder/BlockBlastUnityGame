using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class BlockDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BlockDefinition definition;
    public GridManager gridManager;
    public GridLogic gridLogic;
    public BlockRenderer rendererComp;
    public System.Action onPlaced;
    [HideInInspector] public int rotationSteps;

    private Vector3 startWorldPos;
    private Vector3 startLocalPos;
    private Transform startParent;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rendererComp == null) rendererComp = GetComponent<BlockRenderer>();
    }

    private void OnEnable()
    {
        if (gridManager != null)
            gridManager.OnGridResized += RenderBlock;

        StartCoroutine(RenderNextFrame());
    }

    private void OnDisable()
    {
        if (gridManager != null)
            gridManager.OnGridResized -= RenderBlock;
    }

    private IEnumerator RenderNextFrame()
    {
        yield return null;
        RenderBlock();
    }

    private void OnRectTransformDimensionsChange()
    {
        RenderBlock();
    }

    private void RenderBlock()
    {
        if (gridManager == null || definition == null || rendererComp == null) return;
        float cellSize = gridManager.GetCellSize();
        rendererComp.Render(definition, cellSize, rotationSteps);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gridManager == null) return;

        startWorldPos = rectTransform.position;
        startLocalPos = rectTransform.localPosition;
        startParent = transform.parent;

        Canvas rootCanvas = gridManager.GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
        }

        RenderBlock();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (gridManager == null) return;

        var canvas = gridManager.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var canvasRect = canvas.transform as RectTransform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, cam, out var localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (gridManager == null) return;

        RectTransform board = gridManager.GetBoardRect();
        var canvas = gridManager.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(board, eventData.position, cam, out var localPoint))
        {
            localPoint.x += board.rect.width * 0.5f;
            localPoint.y += board.rect.height * 0.5f;

            float cellSize = gridManager.GetCellSize();
            Vector2 offset = gridManager.GetGridOffset();

            Vector2Int? bestBase = null;
            float bestDist = float.MaxValue;

            Vector2Int blockSize = definition.GetBounds(rotationSteps);
            Vector2 halfBlockSize = new Vector2(blockSize.x * cellSize * 0.5f, blockSize.y * cellSize * 0.5f);

            foreach (var cell in definition.GetNormalizedCells(rotationSteps))
            {
                Vector2 cellPos = localPoint - halfBlockSize + (Vector2)cell * cellSize;

                int gx = Mathf.RoundToInt((cellPos.x - offset.x) / cellSize);
                int gy = Mathf.RoundToInt((cellPos.y - offset.y) / cellSize);

                Vector2 snapped = new Vector2(offset.x + gx * cellSize, offset.y + gy * cellSize);
                float dist = (snapped - cellPos).sqrMagnitude;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestBase = new Vector2Int(gx - cell.x, gy - cell.y);
                }
            }

            if (bestBase.HasValue && CanPlaceBlock(bestBase.Value.x, bestBase.Value.y))
            {
                PlaceBlock(bestBase.Value.x, bestBase.Value.y);
                return;
            }
        }

        transform.SetParent(startParent, true);
        rectTransform.position = startWorldPos;
        rectTransform.localPosition = startLocalPos;
        RenderBlock();
    }

    private bool CanPlaceBlock(int baseX, int baseY)
    {
        if (definition == null || gridManager == null) return false;

        foreach (var cell in definition.GetNormalizedCells(rotationSteps))
        {
            int x = baseX + cell.x;
            int y = baseY + cell.y;

            if (x < 0 || x >= gridManager.width || y < 0 || y >= gridManager.height)
                return false;
            if (gridLogic != null && gridLogic.IsOccupied(x, y))
                return false;
        }
        return true;
    }

    private void PlaceBlock(int baseX, int baseY)
    {
        if (definition == null || gridLogic == null) return;

        foreach (var cell in definition.GetNormalizedCells(rotationSteps))
        {
            int x = baseX + cell.x;
            int y = baseY + cell.y;
            gridLogic.OccupyCell(x, y, Color.white);
        }

        gridLogic.CheckAndClearLines();

        // Gọi callback báo đã đặt xong
        onPlaced?.Invoke();

        // Khi spawn lại sẽ SetActive(true)
        gameObject.SetActive(false);
    }
}
