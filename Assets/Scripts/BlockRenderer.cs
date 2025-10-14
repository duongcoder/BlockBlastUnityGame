using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class BlockRenderer : MonoBehaviour
{
    public Sprite blockSprite;
    public float sizeMultiplier = 1f;

    private Image image;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Render(BlockDefinition definition, float cellSize, int rotationSteps = 0)
    {
        if (image == null || definition == null) return;

        Sprite spriteToUse = definition.sprite != null ? definition.sprite : blockSprite;

        if (spriteToUse != null)
        {
            image.sprite = spriteToUse;
            image.enabled = true;
            image.preserveAspect = true;
        }
        else
        {
            image.enabled = false;
            return;
        }

        // Lấy cell sau khi xoay
        List<Vector2Int> rotatedCells = definition.GetRotatedCells(rotationSteps);

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var c in rotatedCells)
        {
            if (c.x < minX) minX = c.x;
            if (c.x > maxX) maxX = c.x;
            if (c.y < minY) minY = c.y;
            if (c.y > maxY) maxY = c.y;
        }

        int width = (maxX - minX + 1);
        int height = (maxY - minY + 1);

        float totalW = width * cellSize * sizeMultiplier;
        float totalH = height * cellSize * sizeMultiplier;
        rectTransform.sizeDelta = new Vector2(totalW, totalH);

        rectTransform.localRotation = Quaternion.Euler(0, 0, rotationSteps * 90);
    }

    // Xoay cell theo rotationSteps
    private List<Vector2Int> GetRotatedCells(IEnumerable<Vector2Int> originalCells, int steps)
    {
        var rotated = new List<Vector2Int>();
        steps = ((steps % 4) + 4) % 4; // đảm bảo 0..3

        foreach (var cell in originalCells)
        {
            Vector2Int newCell;
            switch (steps)
            {
                case 1: newCell = new Vector2Int(-cell.y, cell.x); break;   // 90°
                case 2: newCell = new Vector2Int(-cell.x, -cell.y); break;  // 180°
                case 3: newCell = new Vector2Int(cell.y, -cell.x); break;   // 270°
                default: newCell = cell; break;                             // 0°
            }
            rotated.Add(newCell);
        }
        return rotated;
    }
}