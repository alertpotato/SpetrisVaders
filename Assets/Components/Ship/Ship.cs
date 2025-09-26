using UnityEngine;
using System.Collections.Generic;

public class Ship : MonoBehaviour
{
    public List<ShipModule> modules = new List<ShipModule>();
    
    public float Speed = 1;

    private void UpdateStats()
    {
        Speed = 1f;

        foreach (var module in modules)
        {
            Speed += module.speedBonus;
        }
    }

    public void AttachModule(ShipModule newModule, Vector2Int gridPos)
    {
        newModule.transform.SetParent(transform);
        newModule.transform.localPosition = new Vector3(gridPos.x, gridPos.y, 0);
        modules.Add(newModule);
        UpdateStats();
    }
}