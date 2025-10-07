using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Block/Definition")]
public class BlockDefinition : ScriptableObject
{
    public Sprite sprite;
    public List<Vector2Int> shapeCells = new List<Vector2Int>();

    public int GetWidth()
    {
        if (shapeCells == null || shapeCells.Count == 0) return 0;

        int maxX = int.MinValue;
        int minX = int.MaxValue;

        foreach (var cell in shapeCells)
        {
            if (cell.x > maxX) maxX = cell.x;
            if (cell.x < minX) minX = cell.x;
        }

        return (maxX - minX + 1);
    }

    public int GetHeight()
    {
        if (shapeCells == null || shapeCells.Count == 0) return 0;

        int maxY = int.MinValue;
        int minY = int.MaxValue;

        foreach (var cell in shapeCells)
        {
            if (cell.y > maxY) maxY = cell.y;
            if (cell.y < minY) minY = cell.y;
        }

        return (maxY - minY + 1);
    }

    public List<Vector2Int> GetNormalizedCells()
    {
        var normalized = new List<Vector2Int>();
        if (shapeCells == null || shapeCells.Count == 0) return normalized;

        int minX = int.MaxValue;
        int minY = int.MaxValue;

        foreach (var cell in shapeCells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.y < minY) minY = cell.y;
        }

        foreach (var cell in shapeCells)
        {
            normalized.Add(new Vector2Int(cell.x - minX, cell.y - minY));
        }

        return normalized;
    }
}
