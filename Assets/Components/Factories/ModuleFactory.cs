using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModuleFactory : MonoBehaviour
{
    private Dictionary<int, int> outfitNumberWeignts;
    private Dictionary<ModuleType, int> defaulModuleWeignts;
    public ShipModuleData[] allModules;

    public GameObject modulePrefab;
    public int moduleCount;

    void Awake()
    {
        outfitNumberWeignts = new Dictionary<int, int>
        {
            { 1, 1000 },
            { 2, 150 },
            { 3, 15 },
            { 4, 1 }
        };
        moduleCount = 0;
        defaulModuleWeignts = new Dictionary<ModuleType, int>();
        foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
        {
            if(type==ModuleType.Empty) continue;
            defaulModuleWeignts[type] = 1;
        }
    }

    public GameObject GetModule(string moduleName = null, Transform parent = null,Dictionary<ModuleType, int> moduleWeights = null)
    {
        moduleCount++;
        var offCameraPoint = new Vector3(-999, -999, 0);
        ShipModuleData data = GetModuleData(name:moduleName,moduleWeights:moduleWeights);
        if (data == null)
        {
            Debug.LogError("No module found: " + moduleName);
            return null;
        }
        // Outfit randomization
        int[] keys = outfitNumberWeignts.Keys.ToArray();
        int[] weights = outfitNumberWeignts.Values.ToArray();

        int index = WeightFunctions.GetRandomWeightedIndex(weights);
        int outfitsNumber = keys[index];
        var outfitPositions = OutfitRandomizer(outfitsNumber);
        // Data class
        var actualData = new ShipModuleStats(data, outfitPositions);
        int rotation = Random.Range(0, 4) * 90;
        GameObject obj = Instantiate(modulePrefab, offCameraPoint, Quaternion.identity, parent);
        ShipModule moduleS = obj.GetComponent<ShipModule>();
        moduleS.Initialize(actualData,rotation);
        obj.name = $"M_{data.moduleName}_{outfitsNumber}_{moduleCount}";
        return obj;
    }

    private ShipModuleData GetModuleData(string name = null,Dictionary<ModuleType, int> moduleWeights = null)
    {
        if (name != null)
        {
            foreach (var d in allModules)
            {
                if (d.moduleName == name) return d;
            }
        }
        if (moduleWeights == null) moduleWeights = defaulModuleWeignts;
        ModuleType chosen = WeightFunctions.GetRandomWeightedModule(moduleWeights);
        foreach (var d in allModules)
        {
            if (d.type == chosen) return d;
        }
        return null;
    }
    public GameObject SpawnRandomModule()
    {
        if (allModules == null || allModules.Length == 0)
        {
            Debug.LogWarning("No modules assigned to factory!");
            return null;
        }
        
        var randomData = allModules[Random.Range(0, allModules.Length)];

        Vector3 pos = Camera.main.ViewportToWorldPoint(
            new Vector3(Random.value, Random.value, 10f) 
        );

        var obj = GetModule(randomData.moduleName);
        obj.transform.SetParent(transform);
        return obj;
    }
    public static int[] OutfitRandomizer(int count)
    {
        count = Mathf.Clamp(count, 0, 4);
        List<int> all = new List<int> { 0, 1, 2, 3 };
        List<int> chosen = new List<int>();

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, all.Count);
            chosen.Add(all[idx]);
            all.RemoveAt(idx);
        }

        return chosen.ToArray();
    }
}


    
