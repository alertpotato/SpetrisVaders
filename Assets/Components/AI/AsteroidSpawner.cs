using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidSpawner : MonoBehaviour
{
    public static AsteroidSpawner Instance;
    public GameObject asteroidPrefab;
    [Header("Components")]
    public Dictionary<GameObject, InertialBody> asteroids = new();
    public GameObject asteroidParent;
    [Header("Variables")]
    public float spawnInterval = 0f;
    private float spawnTimer;
    void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        spawnTimer = Random.Range(60f, 240f);
    }

    void Update()
    {
        //Spawn logic
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            spawnInterval = Random.Range(120f, 240f);
            SpawnEntity();
        }
        CleanupEntities();
    }

    void FixedUpdate()
    {
        foreach (var kvp in asteroids)
        {
            kvp.Value.Tick(Time.fixedDeltaTime);
            kvp.Key.transform.Rotate(new Vector3(0f, 0f, 1f), Time.fixedDeltaTime * kvp.Key.GetComponent<Asteroid>().anglesPerSecond);
        }
    }

    void SpawnEntity()
    {
        Vector2 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        int directionSign = 1;
        if (Random.value < 0.5f) directionSign = -1;

        float randomX = 0.5f + (0.7f * directionSign);
        float randomY = Random.Range(0.1f,0.9f);

        GameObject asteroid = Instantiate(asteroidPrefab,asteroidParent.transform);
        asteroid.transform.position =  Camera.main.ViewportToWorldPoint(new Vector3(randomX, randomY, 10));

        var body = asteroid.GetComponent<InertialBody>();

        body.velocity = Vector2.left * directionSign * Random.Range(0.2f, 3f);
        asteroid.GetComponent<Asteroid>().anglesPerSecond = Random.Range(3f, 15f);
        asteroid.GetComponent<Asteroid>().health = Random.Range(100, 200f);
        asteroids.Add(asteroid, body);
    }

    void CleanupEntities()
    {
        Vector2 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        List<GameObject> toRemove = new();

        foreach (var kvp in asteroids)
        {
            GameObject module = kvp.Key;
            if (module == null)
            {
                toRemove.Add(module);
                continue;
            }
            if (module.transform.position.y < screenMin.x - screenMax.x * 0.3f)
            {
                toRemove.Add(module);
            }
        }

        foreach (var m in toRemove)
        {
            if (m != null) Destroy(m);
            asteroids.Remove(m);
        }
    }
    public void ForgetEntity(GameObject entity)
    {
        asteroids.Remove(entity);
    }

    private void OnDisable()
    {
        var newList = asteroids.Keys.ToList();
        foreach (var obj in newList)
        {
            Destroy(obj);
            asteroids.Remove(obj);
        }
    }
}
