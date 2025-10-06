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
        var offCameraPoint = new Vector3(-999, -999, 0);
        GameObject ship = Instantiate(shipPrefab, offCameraPoint, Quaternion.identity, this.transform);
        var ShipScript = ship.GetComponent<Ship>();
        ShipScript.InitializeShip(Faction.Neutral);
        var cockpit = modules.GetCockpitModule();
        RandomAttach(ShipScript,out GameObject module,cockpit);
        ship.name = $"Ship_{shipCount}";
        return ship;
    }
    public GameObject GetShip(Dictionary<ModuleType, int> moduleWeights = null,int numberOfModules = 3)
    {
        int failStatePreventor =  numberOfModules*50;
        int counter = 0;

        GameObject ship = GetShip();
        var ShipScript = ship.GetComponent<Ship>();
        ShipScript.InitializeShip(Faction.EvilFleet);
        
        while (ShipScript.modules.Count < numberOfModules+1)
        {
            counter++;
            if (counter >= failStatePreventor)
            {
                Debug.LogWarning($"Failed to create ship with {failStatePreventor} itrerations");
                break;
            }

            if (!RandomAttach(ShipScript, out GameObject module, moduleWeights: moduleWeights))
            {
                Destroy(module);
            }
        }

        string m = "";
        foreach (var module in ShipScript.modules)
        {
            if (module.data.type== ModuleType.Cockpit) continue;
            m += module.data.moduleName[0];
        }
        ship.name = $"Ship_{m}_{shipCount}";
        return ship;
    }
    public bool RandomAttach(Ship ship,out GameObject newModule,GameObject moduleToAttach=null,Dictionary<ModuleType, int> moduleWeights = null)
    {
        bool attached = false;
        newModule = moduleToAttach==null? modules.GetModule(moduleWeights: moduleWeights):moduleToAttach;
        var module = newModule.GetComponent<ShipModule>();
        var borderEmptyCells = ship.grid.GetBorderEmptyCells().ToList();
        var anchorList = new List<AnchorOption>();
        foreach (var anchor in SortFunctions.ShuffleList(borderEmptyCells))
        {
            if (ship.grid.TryGetAttachPosition(module, anchor, out var attachAdjustment, module.currentRotation))
            {
                anchorList.Add(new AnchorOption(anchor, attachAdjustment));
                var moduleCandidate = new Candidate(newModule, anchorList);
                ship.AttachModule(moduleCandidate);
                attached = true;
                break;
            }
        }
        return attached;
    }
}