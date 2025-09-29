using UnityEngine;
using System.Collections.Generic;

public class Ship : MonoBehaviour
{
    public List<ShipModule> modules = new List<ShipModule>();
    public ShipGrid grid;
    public float Speed = 1;
    [SerializeField]private GameObject cellPrefab;
    [SerializeField]private Sprite sprite1;
    
    private void UpdateStats()
    {
        Speed = 5f;

        foreach (var module in modules)
        {
            Speed += module.speedBonus;
        }
    }

    public void AttachModule(Candidate candidateModule)
    {
        grid.Attach(candidateModule.module.GetComponent<ShipModule>(), candidateModule.anchor,candidateModule.adjustment,0);
        candidateModule.module.transform.SetParent(transform);
        candidateModule.module.transform.localPosition = new Vector3(candidateModule.anchor.x+candidateModule.adjustment.x, candidateModule.anchor.y+candidateModule.adjustment.y, 0);
        modules.Add(candidateModule.module.GetComponent<ShipModule>());
        UpdateStats();
    }

    public void VizualizeBorders()
    {
        foreach (var cellCoords in grid.GetBorderEmptyCells())
        {
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.GetComponent<ModuleCellScript>().Initialize(sprite1,null);
            cell.transform.localPosition = new Vector3(cellCoords.x, cellCoords.y, 0);
        }
    }
}