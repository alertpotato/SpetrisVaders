using UnityEngine;
using System.Collections.Generic;

public class Ship : MonoBehaviour
{
    public List<ShipModule> modules = new List<ShipModule>();
    public ShipGrid grid;
    public float Speed = 1;
    [SerializeField]private GameObject cellPrefab;
    [SerializeField] private Sprite sprite1;

    private void UpdateStats()
    {
        Speed = 5f;

        foreach (var module in modules)
        {
            Speed += module.speedBonus;
        }
    }

    public void AttachModule(ShipModule newModule, Vector2Int gridPos)
    {
        grid.Attach(newModule, gridPos,0);
        newModule.transform.SetParent(transform);
        newModule.transform.localPosition = new Vector3(gridPos.x, gridPos.y, 0);
        modules.Add(newModule);
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