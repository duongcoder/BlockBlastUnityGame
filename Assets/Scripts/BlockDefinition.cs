using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Block/Definition")]
public class BlockDefinition : ScriptableObject
{
    public Sprite sprite;
    public List<Vector2Int> shapeCells;
}
