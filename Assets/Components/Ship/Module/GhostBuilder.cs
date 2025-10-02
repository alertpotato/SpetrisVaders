using UnityEngine;
using System.Collections.Generic;
public class GhostBuilder : MonoBehaviour
{
    public ShipModuleStats data;
    public GameObject cellPrefab;
    public List<GameObject> cells;
    public int rotation;
    
    public void Initialize(ShipModuleStats newData,int newRotation)
    {
        data = newData;
        rotation = newRotation;
        BuildModule();
    }
    void BuildModule()
    {
        if (data == null || cellPrefab == null) return;
        
        for (int i = 0; i < data.shape.Length; i++)
        {
            Vector2Int pos = RotateCell(data.shape[i].localPosition,rotation);
            Sprite backSprite = data.mainSprite;
            Sprite outfitSprite = null;
            if (data.shape[i].type != OutfitType.Empty) 
            {backSprite = data.backSprite; outfitSprite = data.outfitSprite;}
            
            GameObject cell = Instantiate(cellPrefab, transform);
            cell.GetComponent<ModuleCellScript>().Initialize(backSprite,outfitSprite,0.5f);
            cell.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            cells.Add(cell);
        }
    }
    private Vector2Int RotateCell(Vector2Int cell, int rotation)
    {
        switch (rotation % 360)
        {
            case 90:  return new Vector2Int(-cell.y,  cell.x);
            case 180: return new Vector2Int(-cell.x, -cell.y);
            case 270: return new Vector2Int( cell.y, -cell.x);
            default:  return cell;
        }
    }
    public void AdjustToCell(int cellIndex)
    {
        transform.localPosition += new Vector3(-data.shape[cellIndex].localPosition.x,-data.shape[cellIndex].localPosition.y,0);
    }

}