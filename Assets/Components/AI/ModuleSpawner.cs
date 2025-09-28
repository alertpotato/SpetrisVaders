using System.Collections.Generic;
using UnityEngine;

public class ModuleSpawner : MonoBehaviour
{
    public ModuleFactory MFactory;
    public float spawnInterval = 0f;
    public Dictionary<GameObject, float> modules = new Dictionary<GameObject, float>();
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            spawnInterval = Random.Range(1f, 5f);
            SpawnModule();
        }

        foreach (KeyValuePair<GameObject, float> module in modules)
        {
            module.Key.transform.position += Vector3.down * module.Value * Time.deltaTime;
        }

        foreach (KeyValuePair<GameObject, float> module in modules)
        {
            
        }

        Vector2 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        List<GameObject> toRemove = new List<GameObject>();

        foreach (var kvp in modules)
        {
            GameObject module = kvp.Key;
            if (module == null)
            {
                toRemove.Add(module);
                continue;
            }
            if (module.transform.position.y < screenMin.y - screenMax.y*0.3f)
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

    void SpawnModule()
    {
        Vector2 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        
        float randomX = Random.Range(screenMin.x + screenMax.x*0.15f, screenMax.x - screenMax.x*0.15f);
        float y = screenMax.y*1.3f; // чуть выше экрана, чтобы красиво влетали

        GameObject module = MFactory.GetModule();
        module.transform.position = new Vector3(randomX, y, module.transform.position.z);
        module.transform.SetParent(transform);
        modules.Add(module,Random.Range(1f,3f));
    }
    
}