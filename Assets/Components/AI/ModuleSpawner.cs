using System.Collections.Generic;
using UnityEngine;

public class ModuleSpawner : MonoBehaviour
{
    public static ModuleSpawner Instance;
    [Header("Components")]
    public ModuleFactory MFactory;
    public Dictionary<GameObject, InertialBody> modules = new();
    [Header("Variables")]
    public float spawnInterval = 0f;
    private float spawnTimer;
    void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        CleanupModules();
        //return;
        //Spawn logic
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            spawnInterval = Random.Range(1f, 5f);
            SpawnModule();
        }
    }

    void FixedUpdate()
    {
        foreach (var kvp in modules)
        {
            kvp.Value.Tick(Time.fixedDeltaTime);
        }
    }

    void SpawnModule()
    {
        Vector2 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        
        float randomX = Random.Range(screenMin.x + screenMax.x * 0.15f, screenMax.x - screenMax.x * 0.15f);
        float y = screenMax.y * 1.3f;

        GameObject module = MFactory.GetModule();
        module.transform.position = new Vector3(randomX, y, 0);
        module.transform.SetParent(transform);

        var body = module.GetComponent<InertialBody>();
        body.mass = 1f;
        body.drag = 1f;
        body.maxSpeed = 10f;

        body.velocity = Vector2.down * Random.Range(0.3f, 4f);
        module.layer = LayerMask.NameToLayer(GameLogic.Instance.environmentLayer);
        modules.Add(module, body);
    }

    void CleanupModules()
    {
        Vector2 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        List<GameObject> toRemove = new();

        foreach (var kvp in modules)
        {
            GameObject module = kvp.Key;
            if (module == null)
            {
                toRemove.Add(module);
                continue;
            }
            if (module.transform.position.y < screenMin.y - screenMax.y * 0.3f)
            {
                toRemove.Add(module);
            }
        }

        foreach (var m in toRemove)
        {
            if (m != null) Destroy(m);
            modules.Remove(m);
        }
    }
    public void AddModule(GameObject module,Vector2 direction)
    {
        module.transform.SetParent(transform);

        var body = module.GetComponent<InertialBody>();
        body.mass = 1f;
        body.drag = 1f;
        body.maxSpeed = 10f;

        body.velocity = direction * Random.Range(0.3f, 4f);
        module.layer = LayerMask.NameToLayer(GameLogic.Instance.environmentLayer);
        modules.Add(module, body);
    }

    public void ForgetModule(GameObject module)
    {
        modules.Remove(module);
    }
}
