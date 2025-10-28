using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Block/Definition")]
public class BlockDefinition : ScriptableObject
{
    public Sprite sprite;
    public Color blockColor = Color.white;
    public List<Vector2Int> shapeCells = new List<Vector2Int>();

    // Lấy chiều rộng (số ô) sau khi xoay
    public int GetWidth(int rotationSteps = 0)
    {
        var cells = GetNormalizedCells(rotationSteps);
        if (cells.Count == 0) return 0;

        int maxX = int.MinValue;
        foreach (var c in cells)
            if (c.x > maxX) maxX = c.x;

        return maxX + 1;
    }

    // Lấy chiều cao (số ô) sau khi xoay
    public int GetHeight(int rotationSteps = 0)
    {
        var cells = GetNormalizedCells(rotationSteps);
        if (cells.Count == 0) return 0;

        int maxY = int.MinValue;
        foreach (var c in cells)
            if (c.y > maxY) maxY = c.y;

        return maxY + 1;
    }

    // List cell đã chuẩn hóa (gốc về 0,0)
    public List<Vector2Int> GetNormalizedCells(int rotationSteps = 0)
    {
        var rotated = GetRotatedCells(rotationSteps);
        if (rotated.Count == 0) return new List<Vector2Int>();

        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var c in rotated)
        {
            if (c.x < minX) minX = c.x;
            if (c.y < minY) minY = c.y;
        }

        var normalized = new List<Vector2Int>(rotated.Count);
        foreach (var c in rotated)
            normalized.Add(new Vector2Int(c.x - minX, c.y - minY));

        return normalized;
    }

    // Xoay cell theo rotationSteps (0,1,2,3) = (0°,90°,180°,270°)
    public List<Vector2Int> GetRotatedCells(int rotationSteps)
    {
        var rotated = new List<Vector2Int>();
        if (shapeCells == null || shapeCells.Count == 0) return rotated;

        rotationSteps = ((rotationSteps % 4) + 4) % 4;

        foreach (var cell in shapeCells)
        {
            Vector2Int newCell;
            switch (rotationSteps)
            {
                case 1: newCell = new Vector2Int(-cell.y, cell.x); break;
                case 2: newCell = new Vector2Int(-cell.x, -cell.y); break;
                case 3: newCell = new Vector2Int(cell.y, -cell.x); break;
                default: newCell = cell; break;
            }
            rotated.Add(newCell);
        }
        return rotated;
    }

    // Kiểm tra shape có hợp lệ không (không trùng lặp)
    public bool ValidateShape()
    {
        if (shapeCells == null || shapeCells.Count == 0) return false;

        HashSet<Vector2Int> unique = new HashSet<Vector2Int>(shapeCells);
        if (unique.Count != shapeCells.Count)
        {
            Debug.LogWarning($"[BlockDefinition] {name} có cell trùng lặp.");
            return false;
        }
        return true;
    }
}
