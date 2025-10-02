using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ShipFactory : MonoBehaviour
{
    public GameObject shipPrefab;
    public ModuleFactory modules;
    public int shipCount;

    void Awake()
    {
        shipCount = 0;
    }

    public GameObject GetShip()
    {
        shipCount++;
        int numberOfModules = 3;
        var offCameraPoint = new Vector3(-999, -999, 0);
        int failStatePreventor =  numberOfModules*50;
        int counter = 0;
        GameObject ship = Instantiate(shipPrefab, offCameraPoint, Quaternion.identity, this.transform);
        var ShipScript = ship.GetComponent<Ship>();
        
        while (ShipScript.modules.Count < numberOfModules)
        {
            counter++;
            if (counter >= failStatePreventor)
            {
                Debug.LogWarning($"Failed to create ship with {failStatePreventor} itrerations");
                break;
            }
            var newModule = modules.GetModule();
            var ModuleScript = newModule.GetComponent<ShipModule>();
            var borderEmptyCells = ShipScript.grid.GetBorderEmptyCells().ToList();
            var anchorList = new List<AnchorOption>();
            bool attached = false;
            foreach (var anchor in SortFunctions.ShuffleList(borderEmptyCells))
            {
                if (ShipScript.grid.TryGetAttachPosition(ModuleScript, anchor, out var attachAdjustment, ModuleScript.currentRotation))
                {
                    anchorList.Add(new AnchorOption(anchor, attachAdjustment));
                    var moduleCandidate = new Candidate(newModule, anchorList);
                    ShipScript.AttachModule(moduleCandidate);
                    attached = true;
                    break;
                }
            }
            if (!attached) Destroy(newModule);
        }

        string m = "";
        foreach (var module in ShipScript.modules)
        {
            m += module.data.moduleName[0];
        }
        ship.name = $"Ship_{m}_{shipCount}";
        return ship;
    }
}