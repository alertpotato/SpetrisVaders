using UnityEngine;
using System.Collections.Generic;

public class DockingVisualizer : MonoBehaviour
{
    [Header("References")]
    public ShipGrid shipGrid;
    public GameObject anchorPrefab;
    public GameObject ghostPrefab;

    [Header("Settings")]
    public Color anchorColor = Color.gray;
    public Color closestAnchorColor = Color.green;
    public Color ghostColor = new Color(1f, 1f, 1f, 0.3f);

    private List<GameObject> activeAnchors = new();
    private GameObject ghostInstance;
    private LineRenderer line;

    void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 0;
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
