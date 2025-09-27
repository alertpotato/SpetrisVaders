using UnityEngine;
using System.Collections.Generic;
public class GhostBuilder : MonoBehaviour
{
    public ShipModuleStats data;
    public GameObject cellPrefab;
    public List<GameObject> cells;
    
    public void Initialize(ShipModuleStats newData)
    {
        data = newData;
        BuildModule();
    }
    void BuildModule()
    {
        if (data == null || cellPrefab == null) return;
        
        for (int i = 0; i < data.shape.Length; i++)
        {
            Vector2Int pos = data.shape[i].localPosition;
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
    
}