using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Block/Definition")]
public class BlockDefinition : ScriptableObject
{
    public Sprite sprite;
    public List<Vector2Int> shapeCells = new List<Vector2Int>();

    public int GetWidth(int rotationSteps = 0)
    {
        var rotated = GetRotatedCells(rotationSteps);
        if (rotated.Count == 0) return 0;

        int minX = int.MaxValue, maxX = int.MinValue;
        foreach (var c in rotated)
        {
            if (c.x < minX) minX = c.x;
            if (c.x > maxX) maxX = c.x;
        }
        return (maxX - minX + 1);
    }

    public int GetHeight(int rotationSteps = 0)
    {
        var rotated = GetRotatedCells(rotationSteps);
        if (rotated.Count == 0) return 0;

        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (var c in rotated)
        {
            if (c.y < minY) minY = c.y;
            if (c.y > maxY) maxY = c.y;
        }
        return (maxY - minY + 1);
    }

    // List cell đã chuẩn hóa (gốc về 0,0)
    public List<Vector2Int> GetNormalizedCells(int rotationSteps = 0)
    {
        var rotated = GetRotatedCells(rotationSteps);
        var normalized = new List<Vector2Int>();
        if (rotated.Count == 0) return normalized;

        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var c in rotated)
        {
            if (c.x < minX) minX = c.x;
            if (c.y < minY) minY = c.y;
        }

        foreach (var c in rotated)
        {
            normalized.Add(new Vector2Int(c.x - minX, c.y - minY));
        }
        return normalized;
    }

    // Xoay cell theo rotationSteps
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
}
