using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BlockDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BlockDefinition definition;
    public GridManager gridManager;
    public GridLogic gridLogic;
    public System.Action onPlaced;

    private Vector3 startPos;
    private Transform startParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.position;
        startParent = transform.parent;
        transform.SetParent(gridManager.transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransform rt = gridManager.GetComponent<RectTransform>();

        Camera cam = null;
        if (rt.GetComponentInParent<Canvas>().renderMode != RenderMode.ScreenSpaceOverlay)
            cam = Camera.main;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, cam, out localPoint))
        {
            localPoint.x += rt.rect.width * 0.5f;
            localPoint.y += rt.rect.height * 0.5f;

            Vector2 boardSize = rt.rect.size;
            float paddingX = boardSize.x * 0.05f;
            float paddingY = boardSize.y * 0.05f;
            float gridWidth = boardSize.x - 2 * paddingX;
            float gridHeight = boardSize.y - 2 * paddingY;
            float cellWidth = gridWidth / gridManager.width;
            float cellHeight = gridHeight / gridManager.height;

            int baseX = Mathf.FloorToInt((localPoint.x - paddingX) / cellWidth);
            int baseY = Mathf.FloorToInt((localPoint.y - paddingY) / cellHeight);

            if (CanPlaceBlock(baseX, baseY))
            {
                PlaceBlock(baseX, baseY);
                Destroy(gameObject);
                return;
            }
        }

        transform.position = startPos;
        transform.SetParent(startParent);
    }

    private bool CanPlaceBlock(int baseX, int baseY)
    {
        foreach (var cell in definition.shapeCells)
        {
            int x = baseX + cell.x;
            int y = baseY + cell.y;

            if (x < 0 || x >= gridManager.width || y < 0 || y >= gridManager.height)
                return false;
            if (gridLogic.IsOccupied(x, y))
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
            gridLogic.OccupyCell(x, y, Color.magenta);
        }
        gridLogic.CheckAndClearLines();

        onPlaced?.Invoke();
        Destroy(gameObject);
    }
}
