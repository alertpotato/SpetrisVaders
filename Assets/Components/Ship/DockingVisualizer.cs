using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DockingVisualizer : MonoBehaviour
{
    [Header("References")]
    public ShipGrid shipGrid;
    public GameObject anchorPrefab;
    public GameObject ghostPrefab;
    public ModuleSpawner freeModules;
    [Header("Settings")]
    public Color anchorColor = Color.gray;
    public Color closestAnchorColor = Color.green;
    public Color ghostColor = new Color(1f, 1f, 1f, 0.3f);
    public float maxVisualizeDistance = 5f;
    [Header("Components")]
    private List<GameObject> activeAnchors = new();
    private GameObject ghostInstance;
    private LineRenderer line;
    public DockingCandidatesManager candidates = new DockingCandidatesManager();

    void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 0;
    }

    private void Update()
    {
        var allowedCells = shipGrid.GetBorderEmptyCells().ToList();
        if (allowedCells == null || allowedCells.Count == 0) 
        {
            candidates.PurgeMissing(new HashSet<GameObject>(freeModules.modules.Keys));
            return;
        }

        foreach (var kv in freeModules.modules)
        {
            GameObject module = kv.Key;
            if (module == null) continue;

            if (TryGetNearestAnchor(module, allowedCells, maxVisualizeDistance, out var anchor))
            {
                candidates.AddOrUpdate(module, anchor);
            }
            else candidates.Remove(module);
        }
        candidates.PurgeMissing(new HashSet<GameObject>(freeModules.modules.Keys));

        if (candidates.Count > 0)
        {
            UpdateDocking(candidates.GetCandidatesInOrder().First().module.GetComponent<ShipModule>());
        }
        else ClearVisuals();
    }

    public void UpdateDocking(ShipModule floatingModule)
    {
        ClearVisuals();

        if (floatingModule == null) return;
        var allowed = shipGrid.GetAllowedCells();
        if (allowed.Count == 0) return;

        foreach (var pos in allowed)
        {
            var world = shipGrid.GridToWorld(pos);
            var anchor = Instantiate(anchorPrefab, world, Quaternion.identity);
            anchor.GetComponent<SpriteRenderer>().color = anchorColor;
            activeAnchors.Add(anchor);
        }

        Vector2 modulePos = floatingModule.transform.position;
        Vector2Int closest = FindClosestCell(modulePos, allowed);
        var closestAnchor = activeAnchors[allowed.IndexOf(closest)];
        closestAnchor.GetComponent<SpriteRenderer>().color = closestAnchorColor;

        // line
        line.positionCount = 2;
        line.SetPosition(0, floatingModule.transform.position);
        line.SetPosition(1, closestAnchor.transform.position);

        // ghost
        ghostInstance = Instantiate(ghostPrefab, shipGrid.GridToWorld(closest), Quaternion.identity);
        
        foreach (var sr in ghostInstance.GetComponentsInChildren<SpriteRenderer>())
            sr.color = ghostColor;
    }
    public bool TryGetNearestAnchor(GameObject module, IReadOnlyList<Vector2Int> allowedCells, float visualizeDistance, out Vector2Int anchor)
    {
        anchor = default;
        if (module == null || allowedCells == null || allowedCells.Count == 0)
            return false;

        float limitSqr = visualizeDistance * visualizeDistance;
        Vector2 modulePos = module.transform.position;

        float bestSqr = float.MaxValue;
        bool found = false;

        for (int i = 0; i < allowedCells.Count; i++)
        {
            Vector2 worldPos = shipGrid.GridToWorld(allowedCells[i]);
            float sqr = ((Vector2)modulePos - worldPos).sqrMagnitude;

            if (sqr <= limitSqr && sqr < bestSqr)
            {
                bestSqr = sqr;
                anchor = allowedCells[i];
                found = true;
            }
        }

        return found;
    }
    
    public void ClearVisuals()
    {
        foreach (var a in activeAnchors)
            Destroy(a);
        activeAnchors.Clear();

        if (ghostInstance) Destroy(ghostInstance);
        line.positionCount = 0;
    }

    private Vector2Int FindClosestCell(Vector2 modulePos, List<Vector2Int> cells)
    {
        float bestDist = float.MaxValue;
        Vector2Int best = cells[0];
        foreach (var c in cells)
        {
            var world = shipGrid.GridToWorld(c);
            float d = Vector2.Distance(modulePos, world);
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }
        return best;
    }
}
