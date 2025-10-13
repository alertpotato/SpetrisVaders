using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShipGrid : MonoBehaviour
{
    public Dictionary<Vector2Int, ShipModule> grid = new();
    public float cellSize = 1f;

    public bool TryGetAttachPosition(ShipModule module, Vector2Int anchorPoint, out Vector2Int attachAdjustment,int rotation=-1,bool checkHoles =true)
    {
        var cells = module.data.shape;
        attachAdjustment = Vector2Int.zero;
        var actualRotation = (rotation==-1) ? module.currentRotation : rotation;
        //Iterating through every cell on module, to find first that will get us possible attach position
        foreach (var cell in cells)
        {
            attachAdjustment = -Rotate(cell.localPosition,rotation);
            if ( CanAttach(module, anchorPoint, attachAdjustment, actualRotation,checkHoles) ) return true;
        }
        return false;
    }

    public bool CanAttach(ShipModule module, Vector2Int anchorPoint, Vector2Int cellAdjustment, int rotation,bool checkHoles=true)
    {
        // anchorPoint resets our coordinates at empry cell we want to attach to
        // cellAdjustment makes certain cell on new module 0:0 relative
        // rotation takes into consideration current rotation
        var cells = module.data.shape;
        int contacts = 0;
        // temp cells for flood fill check
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(grid.Keys);

        foreach (var cell in cells)
        {
            var onGridPos = Rotate(cell.localPosition, rotation) + cellAdjustment + anchorPoint;

            if (occupied.Contains(onGridPos))
                return false; // пересечение

            if (IsAdjacent(onGridPos))
                contacts++;

            occupied.Add(onGridPos);
        }

        if (grid.Count == 0)
            return true;
        if (contacts < 2)
            return false;

        if (checkHoles && CreatesForbiddenPockets(occupied, maxPocketSize: 4))
            return false;

        return true;
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
    private bool CreatesForbiddenPockets(HashSet<Vector2Int> occupied, int maxPocketSize = 4)
    {
        if (occupied == null || occupied.Count == 0) return false;

        // bounding box с небольшой подушкой (pad=1)
        int pad = 1;
        int minX = occupied.Min(p => p.x) - pad;
        int maxX = occupied.Max(p => p.x) + pad;
        int minY = occupied.Min(p => p.y) - pad;
        int maxY = occupied.Max(p => p.y) + pad;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Собираем все свободные клетки в прямоугольнике
        List<Vector2Int> free = new List<Vector2Int>();
        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            {
                var p = new Vector2Int(x, y);
                if (!occupied.Contains(p)) free.Add(p);
            }

        if (free.Count == 0) return false;

        // Быстрая карта для проверок
        HashSet<Vector2Int> freeSet = new HashSet<Vector2Int>(free);
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // Найти компоненты свободного пространства
        foreach (var start in free)
        {
            if (visited.Contains(start)) continue;

            // BFS для компоненты
            Queue<Vector2Int> q = new Queue<Vector2Int>();
            List<Vector2Int> comp = new List<Vector2Int>();
            q.Enqueue(start);
            visited.Add(start);

            bool touchesBorder = false;

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                comp.Add(p);

                if (p.x == minX || p.x == maxX || p.y == minY || p.y == maxY)
                    touchesBorder = true;

                foreach (var d in dirs)
                {
                    var n = p + d;
                    if (!freeSet.Contains(n) || visited.Contains(n)) continue;
                    visited.Add(n);
                    q.Enqueue(n);
                }
            }

            // 1) Полностью замкнутые компоненты — запрещаем
            if (!touchesBorder)
                return true;

            // 2) Проверяем узкие карманы внутри этой компоненты
            //    — ищем кандидатов: клетки с малой степенью (<=2 соседей в comp)
            if (comp.Count <= maxPocketSize)
            {
                // даже если эта компонента касается границы, но её размер маленький — считаем нежелательной
                return true;
            }

            // индекс для быстрого поиска соседей в компоненте
            HashSet<Vector2Int> compSet = new HashSet<Vector2Int>(comp);

            // Собираем кандидатов — узкие точки (degree <= 2)
            List<Vector2Int> candidates = new List<Vector2Int>();
            foreach (var p in comp)
            {
                int deg = 0;
                foreach (var d in dirs) if (compSet.Contains(p + d)) deg++;
                if (deg <= 2) candidates.Add(p);
            }

            // Для каждого кандидата симулируем блокировку этой клетки и смотрим,
            // не появится ли маленькая изолированная область (<= maxPocketSize), не касающаяся границы.
            foreach (var cand in candidates)
            {
                // BFS из любой точки компоненты кроме cand — считаем достижимые клетки
                HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
                Queue<Vector2Int> q2 = new Queue<Vector2Int>();

                // найдём стартовую точку для BFS: любую соседнюю клетку компоненты (или другую cell)
                Vector2Int start2 = default;
                bool got = false;
                foreach (var p in comp)
                {
                    if (p == cand) continue;
                    start2 = p; got = true; break;
                }
                if (!got) continue;

                q2.Enqueue(start2);
                seen.Add(start2);

                while (q2.Count > 0)
                {
                    var p = q2.Dequeue();
                    foreach (var d in dirs)
                    {
                        var n = p + d;
                        if (n == cand) continue;                 // симулируем блокировку
                        if (!compSet.Contains(n) || seen.Contains(n)) continue;
                        seen.Add(n);
                        // ранний выход — если область больше порога, нас это не интересует
                        if (seen.Count > maxPocketSize + 2) { q2.Clear(); break; }
                        q2.Enqueue(n);
                    }
                }

                // если после блокировки видим, что часть компоненты (comp.Count - seen.Count) <= maxPocketSize
                // и эта "потерянная" часть НЕ касается границы => это узкий карман
                int lostCount = comp.Count - seen.Count;
                if (lostCount > 0 && lostCount <= maxPocketSize)
                {
                    // собрать одну клетку из "потерянной" и проверить её касание границы
                    HashSet<Vector2Int> lostSet = new HashSet<Vector2Int>(compSet.Except(seen));
                    bool lostTouchesBorder = false;
                    foreach (var p in lostSet)
                    {
                        if (p.x == minX || p.x == maxX || p.y == minY || p.y == maxY) { lostTouchesBorder = true; break; }
                    }
                    if (!lostTouchesBorder) return true;
                }
            }
        }
        return false;
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