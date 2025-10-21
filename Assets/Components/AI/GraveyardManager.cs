using System.Collections.Generic;
using UnityEngine;

public class GraveyardManager : MonoBehaviour
{
    [Header("Components")]
    public static GraveyardManager Instance;
    public GameObject GraveParent;
    [Header("Variables")]
    public Dictionary<GameObject, InertialBody> modules = new();
    public float saveModuleChance = 0.05f;
    public float scaleLoss = 0.997f;
    void Awake()
    {
        Instance = this;
    }
    void FixedUpdate()
    {
        var moduleToForget = new List<GameObject>();
        foreach (var kvp in modules)
        {
            kvp.Value.Tick(Time.fixedDeltaTime);
            kvp.Key.transform.localScale *= scaleLoss;
            if (kvp.Key.transform.localScale.magnitude < 0.05f)
            {
                moduleToForget.Add(kvp.Key);
            }
        }
        foreach (var module in moduleToForget)
        {
            ForgetModule(module);
            Destroy(module);
        }
    }
    public void DecideOnDestroyedModules(List<ShipModule> shipModules, Ship ship)
    {
        Vector3 shipCenter = ship.grid.GridToWorld(ship.GetGridCenterLocal());
        foreach (var shipModule in shipModules)
        {
            Vector2 direction = (Vector2)(shipModule.transform.position - shipCenter).normalized;
            
            if (Random.value < saveModuleChance) ModuleSpawner.Instance.AddModule(shipModule.gameObject, direction);
            else AddModule(shipModule.gameObject, direction);
        }
    }
    
    public void AddModule(GameObject module,Vector2 direction)
    {
        module.transform.SetParent(GraveParent.transform);

        var body = module.GetComponent<InertialBody>();
        body.mass = 1f;
        body.drag = 1f;
        body.maxSpeed = 10f;

        body.velocity = direction * Random.Range(1f, 4f);
        module.layer = LayerMask.NameToLayer(GameLogic.Instance.environmentLayer);
        modules.Add(module, body);
    }

    public void ForgetModule(GameObject module)
    {
        modules.Remove(module);
    }
}