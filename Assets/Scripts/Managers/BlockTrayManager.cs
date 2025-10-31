using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class BlockTrayManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private bool logDebug = false;
    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = Vector2.zero;
    }

    private void Start()
    {
        if (gridManager != null)
        {
            gridManager.OnGridResized += ResizeTray;
            ResizeTray();
        }
        else 
        {
            Debug.LogWarning("[BlockTrayManager] GridManager is not allocated in Inspector.");
        }
    }

    private void OnDestroy()
    {
        if (gridManager != null)
            gridManager.OnGridResized -= ResizeTray;
    }

    void OnRectTransformDimensionsChange()
    {
        ResizeTray();
    }

    private void ResizeTray()
    {
        if (rt == null || rt.parent == null || gridManager == null) return;

        RectTransform parent = rt.parent as RectTransform;

        float trayHeight = gridManager.GetBoardBottomLocal(parent);
        if (trayHeight < 0) trayHeight = 0;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, trayHeight);

        if (logDebug)
        {
            Debug.Log($"[Tray] parentHeight={parent.rect.height}, trayHeight={trayHeight}, rtHeight={rt.rect.height}");
        }
    }
}
