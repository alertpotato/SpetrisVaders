using UnityEngine;
using System.Collections.Generic;
public class ModuleBuilder : MonoBehaviour
{
    public ShipModuleStats data;
    public GameObject cellPrefab;
    public List<GameObject> cells;
    
    public void Initialize(ShipModuleStats newData,int currentRotation=0)
    {
        data = newData;
        BuildModule(currentRotation);
    }
    void BuildModule(int currentRotation)
    {
        if (data == null || cellPrefab == null) return;
        
        for (int i = 0; i < data.shape.Length; i++)
        {
            Vector2Int pos = RotateCell(data.shape[i].localPosition,currentRotation);
            Sprite backSprite = data.mainSprite;
            Sprite outfitSprite = null;
            if (data.shape[i].type != OutfitType.Empty)
            {backSprite = data.backSprite; outfitSprite = data.outfitSprite;}
            
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.GetComponent<ModuleCellScript>().Initialize(backSprite,outfitSprite);
            cell.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            cells.Add(cell);
        }
    }
    public Vector2Int RotateCell(Vector2Int cell, int rotation)
    {
        switch (rotation % 360)
        {
            case 90:  return new Vector2Int(-cell.y,  cell.x);
            case 180: return new Vector2Int(-cell.x, -cell.y);
            case 270: return new Vector2Int( cell.y, -cell.x);
            default:  return cell;
        }
    }
    public void UpdateModule(int newRotation)
    {
        if (data == null || cellPrefab == null) return;

        for (int i = 0; i < data.shape.Length; i++)
        {
            Vector2Int pos = RotateCell(data.shape[i].localPosition,newRotation);
            cells[i].transform.localPosition = new Vector3(pos.x, pos.y, 0);;
        }
    }

}