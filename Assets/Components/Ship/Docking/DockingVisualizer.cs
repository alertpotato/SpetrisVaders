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
    public int maxVisualizeAnchors = 10;
    [Header("Components")]
    public GameObject AnchorParent;
    private List<GameObject> activeAnchors = new();
    public GameObject ghostInstance;
    private LineRenderer line;
    public DockingCandidatesManager candidates = new DockingCandidatesManager();
    [Header("Variables")]
    private GameObject currentShipModule;
    public int currentRotation = 0;
    public bool forbidEmptySpaces = false;
    
    void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 0;
        shipGrid = GetComponent<ShipGrid>();
    }

    public void Initialize(GameObject anchorPref, GameObject ghostPref, ModuleSpawner freeM, GameObject anchorParent)
    {
        anchorPrefab = anchorPref;
        ghostPrefab = ghostPref;
        freeModules = freeM;
        AnchorParent = anchorParent;
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
            
            var anchors = TryGetNearestAnchors(module, allowedCells, maxVisualizeDistance, maxVisualizeAnchors);
            var options = new List<AnchorOption>();
            foreach (var anchor in anchors)
            {
                if (shipGrid.TryGetAttachPosition(module.GetComponent<ShipModule>(), anchor, out var attachAdjustment, currentRotation,forbidEmptySpaces))
                {
                    options.Add(new AnchorOption(anchor, attachAdjustment));
                }
            }

            if (options.Count > 0)
                candidates.AddOrUpdate(module, options);
            else
                candidates.Remove(module);
            
        }
        candidates.PurgeMissing(new HashSet<GameObject>(freeModules.modules.Keys));

        if (candidates.Count > 0)
        {
            var candidate = candidates.GetCandidatesInOrder().First();
            //is same module dont touch rotation
            if (currentShipModule != candidate.module) {
                currentShipModule = candidate.module;
                currentRotation = candidate.module.GetComponent<ShipModule>().currentRotation;
            }
            var primary = candidate.Primary;
            UpdateDocking(candidate.module.GetComponent<ShipModule>(), primary.anchor, primary.adjustment, currentRotation);
        }
        else ClearVisuals();
    }

    public void UpdateDocking(ShipModule floatingModule,Vector2Int anchorPos,Vector2Int anchorAdjustment,int newRotation)
    {
        ClearVisuals();
        
        if (floatingModule == null) return;
        var candidate = candidates.GetCandidatesInOrder().First();
        if (candidate.options.Count == 0) return;
        GameObject closestAnchor = null;
        foreach (var candidateAnchor in candidate.options)
        {
            var anchor = Instantiate(anchorPrefab, AnchorParent.transform);
            anchor.GetComponent<SpriteRenderer>().color = anchorColor;
            anchor.transform.localPosition = new Vector3(candidateAnchor.anchor.x, candidateAnchor.anchor.y, 0);
            activeAnchors.Add(anchor);
            if (candidate.Primary.anchor == candidateAnchor.anchor) closestAnchor = anchor;
        }
    
        Vector2 modulePos = floatingModule.transform.position;
        //TODO nullreferenceexeption possible
        closestAnchor.GetComponent<SpriteRenderer>().color = closestAnchorColor;
        
        // ghost
        ghostInstance = Instantiate(ghostPrefab, transform);
        ghostInstance.transform.localPosition = new Vector3(anchorPos.x + anchorAdjustment.x, anchorPos.y + anchorAdjustment.y, 0);
        ghostInstance.GetComponent<GhostBuilder>().Initialize(floatingModule.data,newRotation);
        
        // line
        line.positionCount = 2;
        line.SetPosition(0, floatingModule.transform.position);
        line.SetPosition(1, closestAnchor.transform.position);
    }
    public List<Vector2Int> TryGetNearestAnchors(
        GameObject module,
        IReadOnlyList<Vector2Int> allowedCells,
        float visualizeDistance,
        int maxCount = 5)
    {
        var result = new List<Vector2Int>();
        if (module == null || allowedCells == null || allowedCells.Count == 0)
            return result;

        float limitSqr = visualizeDistance * visualizeDistance;
        Vector2 modulePos = module.transform.position;

        var candidates = new List<(float sqr, Vector2Int cell)>();

        for (int i = 0; i < allowedCells.Count; i++)
        {
            Vector2 worldPos = shipGrid.GridToWorld(allowedCells[i]);
            float sqr = ((Vector2)modulePos - worldPos).sqrMagnitude;

            if (sqr <= limitSqr)
            {
                candidates.Add((sqr, allowedCells[i]));
            }
        }
        
        candidates.Sort((a, b) => a.sqr.CompareTo(b.sqr));

        for (int i = 0; i < Mathf.Min(maxCount, candidates.Count); i++)
            result.Add(candidates[i].cell);

        return result;
    }
    public void ClearVisuals()
    {
        foreach (var a in activeAnchors)
            Destroy(a);
        activeAnchors.Clear();

        if (ghostInstance) Destroy(ghostInstance);
        line.positionCount = 0;
    }
    public void RotateModule()
    {
        currentRotation = (currentRotation + 90) % 360;
    }
}
