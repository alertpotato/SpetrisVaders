using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShipGrid : MonoBehaviour
{
    public Dictionary<Vector2Int, ShipModule> grid = new();
    public float cellSize = 1f;

    public bool TryGetAttachPosition(ShipModule module, Vector2Int anchorPoint, out Vector2Int attachAdjustment,int rotation=-1)
    {
        var cells = module.data.shape;
        attachAdjustment = Vector2Int.zero;
        var actualRotation = (rotation==-1) ? module.currentRotation : rotation;
        //Iterating through every cell on module, to find first that will get us possible attach position
        foreach (var cell in cells)
        {
            attachAdjustment = -Rotate(cell.localPosition,rotation);
            if ( CanAttach(module, anchorPoint, attachAdjustment, actualRotation) ) return true;
        }
        return false;
    }

    public bool CanAttach(ShipModule module, Vector2Int anchorPoint, Vector2Int cellAdjustment, int rotation)
    {
        // anchorPoint resets our coordinates at empry cell we want to attach to
        // cellAdjustment makes certain cell on new module 0:0 relative
        // rotation takes into consideration current rotation
        var cells = module.data.shape;
        int contacts = 0;

        foreach (var cell in cells)
        {
            var onGridPos = Rotate(cell.localPosition, rotation) + cellAdjustment + anchorPoint;

            if (grid.ContainsKey(onGridPos))
                return false;

            if (IsAdjacent(onGridPos))
                contacts++;
        }

        if (contacts >= 2 || grid.Count==0) return true;

    // TODO: проверка дырок (алгоритм flood fill)
        return false;
    }

    public void Attach(ShipModule module, Vector2Int anchorPosition,Vector2Int anchorAdjustment)
    {
        foreach (var cell in module.data.shape)
        {
            var rotated = Rotate(cell.localPosition, module.currentRotation);
            var worldPos = anchorPosition + rotated + anchorAdjustment;
            grid[worldPos] = module;
        }
        module.gridPosition = anchorPosition;
    }
    public List<ShipModule> GetDisconnectedModules(ShipModule cockpit)
    {
        var result = new List<ShipModule>();
        if (cockpit == null || grid.Count == 0)
            return result;

        var cockpitCells = grid
            .Where(kvp => kvp.Value == cockpit)
            .Select(kvp => kvp.Key)
            .ToList();

        if (cockpitCells.Count == 0)
            return new List<ShipModule>(grid.Values.Distinct());

        // BFS search
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(cockpitCells);
        Queue<Vector2Int> frontier = new Queue<Vector2Int>(cockpitCells);

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = current + dir;
                if (grid.ContainsKey(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    frontier.Enqueue(next);
                }
            }
        }
        
        var disconnected = grid
            .Where(kvp => !visited.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .Distinct()
            .ToList();

        return disconnected;
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
        if (result.Count == 0) result.Add(Vector2Int.zero);
        return result;
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
    public Vector2 GridToWorld(Vector2 cell)
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
    
    public void RemoveModule(ShipModule module)
    {
        if (module == null) return;

        var keysToRemove = new List<Vector2Int>();
        foreach (var kvp in grid)
        {
            if (kvp.Value == module)
                keysToRemove.Add(kvp.Key);
        }
        foreach (var key in keysToRemove)
        {
            grid.Remove(key);
        }
    }
}