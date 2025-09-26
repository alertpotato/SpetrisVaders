using UnityEngine;
using System.Collections.Generic;
public class ShipGrid : MonoBehaviour
{
    private Dictionary<Vector2Int, ShipModule> occupied = new();

    public bool CanAttach(ShipModule module, Vector2Int atPosition, int rotation)
    {
        var cells = module.data.shape;
        int contacts = 0;

        foreach (var cell in cells)
        {
            var rotated = Rotate(cell.localPosition, rotation);
            var worldPos = atPosition + rotated;

            // проверка на наложение
            if (occupied.ContainsKey(worldPos))
                return false;

            // проверка на контакт (сосед по 4 направлениям)
            if (IsAdjacent(worldPos))
                contacts++;
        }

        // минимум 2 касания
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
            occupied[worldPos] = module;
        }
        module.gridPosition = atPosition;
        module.rotation = rotation;
    }

    private bool IsAdjacent(Vector2Int pos)
    {
        return occupied.ContainsKey(pos + Vector2Int.up)
               || occupied.ContainsKey(pos + Vector2Int.down)
               || occupied.ContainsKey(pos + Vector2Int.left)
               || occupied.ContainsKey(pos + Vector2Int.right);
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
}