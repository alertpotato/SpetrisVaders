using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Запускается в сцене; задавай playerShip через инспектор
public class EnemyManager : MonoBehaviour
{
    public Ship playerShip;
    public ShipFactory SFactory;
    public float screenBorderPercent = 0.3f;
    public struct EnemyEntry { public Ship ship; public ShipArchetype archetype; public EnemyEntry(Ship s, ShipArchetype a) { ship = s; archetype = a; } }
    public List<EnemyEntry> enemies = new List<EnemyEntry>();

    Camera ScreenCamera;
    
    void Awake()
    {
        ScreenCamera = Camera.main;
    }

    void FixedUpdate()
    {
        var deltaTime = Time.fixedDeltaTime;
        var playerPos = ScreenCamera.WorldToViewportPoint(playerShip.transform.position);
        
        foreach (var enemy in enemies)
        {
            var enemyPos = ScreenCamera.WorldToViewportPoint(enemy.ship.transform.position);
            var shipVelocity = enemy.archetype.GetVelocity(enemy.ship.inertialBody,enemyPos,playerPos);
            enemy.ship.inertialBody.ApplyForce(shipVelocity * enemy.ship.thrust, deltaTime);
            enemy.ship.inertialBody.Tick(deltaTime,isForceApplied:enemy.archetype.currentDirection==Vector2.zero?false:true);
        }
    }
    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            //GameObjects removal
            var entry = enemies[i];
            Debug.DrawLine(enemies[i].ship.transform.position, ScreenCamera.ViewportToWorldPoint(enemies[i].archetype.currentTarget), Color.green);
            Debug.DrawLine(enemies[i].ship.transform.position, enemies[i].ship.transform.position + ScreenCamera.ViewportToWorldPoint(enemies[i].archetype.currentDirection), Color.blue);
            if (entry.ship == null)
            {
                enemies.RemoveAt(i);
                continue;
            }
            
            if (entry.ship.modules == null || entry.ship.modules.Count == 0)
            {
                GameObject.Destroy(entry.ship.gameObject);
                enemies.RemoveAt(i);
                continue;
            }

            // OutsideBounds ???
            if (IsOutsideBounds(entry.ship.transform.position))
            {
                entry.archetype.OnOutOfBounds(entry.ship, this);
            }

            // ship logic tick
            entry.archetype.Tick(entry.ship, playerShip, dt, this);
        }
    }
    public void SpawnShip()
    {
        Vector2 screenMin = ScreenCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenMax = ScreenCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        
        float randomX = Random.Range(screenMin.x + screenMax.x * 0.15f, screenMax.x - screenMax.x * 0.15f);
        float y = screenMax.y * 1.3f;
        
        var archetype = new FlagshipArchetype();
        var newShip = SFactory.GetShip(archetype.moduleWeights,GetModuleCount(archetype.minModules, archetype.maxModules));
        newShip.transform.SetParent(this.transform);
        RegisterEnemy(newShip.GetComponent<Ship>(),archetype);
        
        newShip.transform.position = new Vector3(randomX, y, 0);
    }
    public void RegisterEnemy(Ship ship, ShipArchetype archetype)
    {
        if (ship == null || archetype == null) return;
        ship.OnDestroyed += HandleShipDestroyed;
        enemies.Add(new EnemyEntry(ship, archetype));
    }
    private void HandleShipDestroyed(Ship destroyedShip)
    {
        RemoveEnemy(destroyedShip);
    }

    public void RemoveEnemy(Ship ship)
    {
        if (ship == null) return;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i].ship == ship)
            {
                enemies.RemoveAt(i);
                break;
            }
        }
    }
    private bool IsOutsideBounds(Vector3 worldPos)
    {
        Vector3 vp = ScreenCamera.WorldToViewportPoint(worldPos);
        return vp.x < -screenBorderPercent || vp.x > 1 + screenBorderPercent ||
               vp.y < -screenBorderPercent || vp.y > 1 + screenBorderPercent;
    }
    public Vector3 GetTopSpawnPositionFor(Ship ship)
    {
        Vector3 topWorld = ScreenCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f));
        return new Vector3(ship.transform.position.x, topWorld.y - 0.5f, 0f);
    }
    int GetModuleCount(int minModules, int maxModules, float difficulty=1)
    {
        int scaledMin = Mathf.RoundToInt(minModules * difficulty);
        int scaledMax = Mathf.RoundToInt(maxModules * difficulty);

        return Random.Range(scaledMin, scaledMax + 1);
    }
}
