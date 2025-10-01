using UnityEngine;

public class GridClickTester : MonoBehaviour
{
    public GridManager gridManager;
    public GridLogic gridLogic;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 localPoint;
            RectTransform rt = gridManager.GetComponent<RectTransform>();

            Camera cam = null;
            if (rt.GetComponentInParent<Canvas>().renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = Camera.main;
            }

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

                int x = Mathf.FloorToInt((localPoint.x - paddingX) / cellWidth);
                int y = Mathf.FloorToInt((localPoint.y - paddingY) / cellHeight);

                Debug.Log($"Click local: {localPoint}, Cell: ({x},{y})");

                if (x >= 0 && x < gridManager.width && y >= 0 && y < gridManager.height)
                {
                    gridLogic.OccupyCell(x, y, Color.red);
                    gridLogic.CheckAndClearLines();
                }
            }
        }
    }
}
