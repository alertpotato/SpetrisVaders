using UnityEngine;
using System.Collections.Generic;
public class ShipGrid : MonoBehaviour
{
    public Dictionary<Vector2Int, ShipModule> grid = new();
    public float cellSize = 1f; 
    
    public bool CanAttach(ShipModule module, Vector2Int atPosition, int rotation)
    {
        var cells = module.data.shape;
        int contacts = 0;

        foreach (var cell in cells)
        {
            var rotated = Rotate(cell.localPosition, rotation);
            var worldPos = atPosition + rotated;

            if (grid.ContainsKey(worldPos))
                return false;

            if (IsAdjacent(worldPos))
                contacts++;
        }

        if (contacts < 2) return false;

        // TODO: проверка дырок (алгоритм flood fill)
        return true;
    }

    public void Attach(ShipModule module, Vector2Int atPosition, int rotation)
    {
        foreach (var cell in module.data.shape)
        {
            var rotated = Rotate(cell.localPosition, rotation);
            var worldPos = atPosition + rotated;
            grid[worldPos] = module;
        }
        module.gridPosition = atPosition;
        module.rotation = rotation;
    }

    private bool IsAdjacent(Vector2Int pos)
    {
        return grid.ContainsKey(pos + Vector2Int.up)
               || grid.ContainsKey(pos + Vector2Int.down)
               || grid.ContainsKey(pos + Vector2Int.left)
               || grid.ContainsKey(pos + Vector2Int.right);
    }

    private Vector2Int Rotate(Vector2Int cell, int rotation)
    {
        switch (rotation % 360)
        {
            case 90:  return new Vector2Int(-cell.y,  cell.x);
            case 180: return new Vector2Int(-cell.x, -cell.y);
            case 270: return new Vector2Int( cell.y, -cell.x);
            default:  return cell;
        }
    }
    public Vector2 GridToWorld(Vector2Int cell)
    {
        return (Vector2)transform.position + new Vector2(cell.x * cellSize, cell.y * cellSize);
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        Vector2 local = worldPos - (Vector2)transform.position;
        return new Vector2Int(
            Mathf.RoundToInt(local.x / cellSize),
            Mathf.RoundToInt(local.y / cellSize)
        );
    }
    public HashSet<Vector2Int> GetBorderEmptyCells()
    {
        HashSet<Vector2Int> result = new HashSet<Vector2Int>();

        foreach (var kvp in grid)
        {
            Vector2Int pos = kvp.Key;
            
            Vector2Int[] neighbors = new Vector2Int[]
            {
                pos + Vector2Int.up,
                pos + Vector2Int.down,
                pos + Vector2Int.left,
                pos + Vector2Int.right
            };

            foreach (var n in neighbors)
            {
                if (!grid.ContainsKey(n))
                {
                    result.Add(n);
                }
            }
        }
        foreach (var cell in result)
        {
            Debug.Log("Empty border cell: " + cell);
        }
        return result;
    }
    public List<Vector2Int> GetAllowedCells()
    {
        var result = new List<Vector2Int>();
        
        Vector2Int center = new Vector2Int(0, 0);

        result.Add(center + Vector2Int.up);
        result.Add(center + Vector2Int.down);
        result.Add(center + Vector2Int.left);
        result.Add(center + Vector2Int.right);

        return result;
    }

}