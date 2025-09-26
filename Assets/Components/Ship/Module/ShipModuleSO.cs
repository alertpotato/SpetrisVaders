using UnityEngine;

[CreateAssetMenu(fileName = "ShipModuleData", menuName = "Game/Ship Module Data")]
public class ShipModuleData : ScriptableObject
{
    public string moduleName;
    public ModuleType type;
    public CellData[] shape;
    public int baseHealth = 4;
    public Sprite mainSprite;
    public Sprite backSprite;
    public Sprite outfitSprite;
}

[System.Serializable]
public struct CellData
{
    public Vector2Int localPosition;
    public OutfitType type;
    public CellData(Vector2Int localPosition, OutfitType type)
    {
        this.localPosition = localPosition;
        this.type = type;
    }
}

[System.Serializable]
public class ShipModuleStats
{
    public string moduleName;
    public ModuleType type;
    public CellData[] shape;
    public int baseHealth;
    public Sprite mainSprite;
    public Sprite backSprite;
    public Sprite outfitSprite;
    
    public ShipModuleStats(ShipModuleData data)
    {
        moduleName = data.moduleName;
        type = data.type;
        shape = (CellData[])data.shape.Clone();
        baseHealth = data.baseHealth;
        mainSprite = data.mainSprite;
        backSprite = data.backSprite;
        outfitSprite = data.outfitSprite;
    }
}
