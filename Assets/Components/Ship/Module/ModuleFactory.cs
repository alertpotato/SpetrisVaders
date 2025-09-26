using UnityEngine;

public class ModuleFactory : MonoBehaviour
{
    public static ModuleFactory Instance;

    public ShipModuleData[] allModules;

    public GameObject modulePrefab;

    void Awake()
    {
        Instance = this;
    }

    public GameObject CreateModule(string moduleName, Vector3 position, Transform parent = null)
    {
        ShipModuleData data = GetModuleData(moduleName);
        if (data == null)
        {
            Debug.LogError("No module found: " + moduleName);
            return null;
        }
        var actualData = new ShipModuleStats(data);

        //actualData.shape[i] = new CellData(actualData.shape[i].localPosition,OutfitType.Canon);

        GameObject obj = Instantiate(modulePrefab, position, Quaternion.identity, parent);
        ShipModule moduleS = obj.GetComponent<ShipModule>();
        moduleS.Initialize(actualData);
        
        return obj;
    }

    private ShipModuleData GetModuleData(string name)
    {
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

        var obj = CreateModule(randomData.moduleName, pos);
        obj.transform.SetParent(transform);
        return obj;
    }

}