using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BlockDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BlockDefinition definition;
    public GridManager gridManager;
    public GridLogic gridLogic;
    public BlockRenderer rendererComp;
    public System.Action onPlaced;

    private Vector3 startPos;
    private Transform startParent;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rendererComp == null) rendererComp = GetComponentInChildren<BlockRenderer>();
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

    private System.Collections.IEnumerator RenderNextFrame()
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
        rendererComp.Render(definition, cellSize);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rectTransform.position;
        startParent = transform.parent;

        Canvas rootCanvas = gridManager.GetComponentInParent<Canvas>();
        if (rootCanvas)
        {
            transform.SetParent(rootCanvas.transform, true);
        }

        RenderBlock();
    }

    public void OnDrag(PointerEventData eventData)
    {
        var canvas = gridManager.GetComponentInParent<Canvas>();
        var canvasRect = canvas.transform as RectTransform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, cam, out localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        RectTransform board = gridManager.GetBoardRect();
        var canvas = gridManager.GetComponentInParent<Canvas>();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(board, eventData.position, cam, out localPoint))
        {
            localPoint.x += board.rect.width * 0.5f;
            localPoint.y += board.rect.height * 0.5f;

            float paddingX = board.rect.size.x * gridManager.paddingRatio.x;
            float paddingY = board.rect.size.y * gridManager.paddingRatio.y;
            float gridWidth = board.rect.size.x - 2 * paddingX;
            float gridHeight = board.rect.size.y - 2 * paddingY;

            float cellW = gridWidth / gridManager.width;
            float cellH = gridHeight / gridManager.height;

            int baseX = Mathf.FloorToInt((localPoint.x - paddingX) / cellW);
            int baseY = Mathf.FloorToInt((localPoint.y - paddingY) / cellH);

            if (CanPlaceBlock(baseX, baseY))
            {
                PlaceBlock(baseX, baseY);
                return;
            }
        }

        rectTransform.position = startPos;
        transform.SetParent(startParent, true);     
    }

    private bool CanPlaceBlock(int baseX, int baseY)
    {
        foreach (var cell in definition.shapeCells)
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
        foreach (var cell in definition.shapeCells)
        {
            int x = baseX + cell.x;
            int y = baseY + cell.y;
            gridLogic?.OccupyCell(x, y, Color.magenta);
        }

        gridLogic?.CheckAndClearLines();

        onPlaced?.Invoke();
        Destroy(gameObject);
    }
}
