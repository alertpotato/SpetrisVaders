using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipModuleData", menuName = "Game/Ship Module Data")]
public class ShipModuleData : ScriptableObject
{
    public string moduleName;
    public ModuleType type;
    public OutfitType outfitType;
    public CellData[] shape;
    public int baseHealth = 4;
    public float cooldown;
    public float speedModifier;
    public int damage;
    public float maxRange;
    public float accuracy; //spread radius on max distance
    public Sprite mainSprite;
    public Sprite backSprite;
    public Sprite outfitSprite;
}
[CreateAssetMenu(fileName = "ShipModuleData", menuName = "Game/Ship Modules Weights")]
public class ShipModuleWeights : ScriptableObject
{
    public string weightName;
    public List<ModuleWeights> weights;
}
[System.Serializable]
public struct ModuleWeights
{
    [Tooltip("Name")]public string name;
    [Tooltip("Weight")]public int weight;
    public ModuleWeights( string moduleName,int moduleWeight)
    {
        name = moduleName;weight=moduleWeight;
    }
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
    public OutfitType outfitType;
    public CellData[] shape;
    public int baseHealth;
    public float cooldown;
    public float speedModifier;
    public int damage;
    public float maxRange;
    public float accuracy;
    public Sprite mainSprite;
    public Sprite backSprite;
    public Sprite outfitSprite;
    
    public ShipModuleStats(ShipModuleData data, int[] outfitPositions=null)
    {
        moduleName = data.moduleName;
        type = data.type;
        outfitType = data.outfitType;
        shape = (CellData[])data.shape.Clone();
        baseHealth = data.baseHealth;
        cooldown = data.cooldown;
        speedModifier = data.speedModifier;
        damage = data.damage;
        maxRange = data.maxRange;
        accuracy = data.accuracy;
        mainSprite = data.mainSprite;
        backSprite = data.backSprite;
        outfitSprite = data.outfitSprite;
        
        ApplyOutfits(outfitPositions,data.outfitType);
    }
    public void ApplyOutfits(int[] indices, OutfitType type)
    {
        if (indices == null || indices.Length == 0) return;
        foreach (int idx in indices)
        {
            if (idx >= 0 && idx < shape.Length)
            {
                shape[idx] = new CellData(shape[idx].localPosition, type);
            }
        }
    }
}
