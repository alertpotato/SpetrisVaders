using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModuleFactory : MonoBehaviour
{
    public static ModuleFactory Instance;
    public Dictionary<int, int> outfitNumberWeignts;
    public ShipModuleData[] allModules;

    public GameObject modulePrefab;

    void Awake()
    {
        Instance = this;
        outfitNumberWeignts = new Dictionary<int, int>
        {
            { 1, 1000 },
            { 2, 150 },
            { 3, 15 },
            { 4, 1 }
        };
    }

    public GameObject GetModule(string moduleName = null, Transform parent = null)
    {
        ShipModuleData data = GetModuleData(moduleName);
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
        GameObject obj = Instantiate(modulePrefab, Vector3.zero, Quaternion.identity, parent);
        ShipModule moduleS = obj.GetComponent<ShipModule>();
        moduleS.Initialize(actualData,rotation);
        
        return obj;
    }

    private ShipModuleData GetModuleData(string name = null)
    {
        if (name == null)
        {
            name = allModules[Random.Range(0, allModules.Length)].moduleName;
        }

        foreach (var d in allModules)
        {
            if (d.moduleName == name) return d;
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


    
