using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public Ship playerShip;
    public ShipFactory SFactory;
    public TypewriterMessageQueue HUDConsole;
    public float screenBorderPercent = 0.3f;
    public List<ShipArchetype> archetypes;
    public struct EnemyEntry { public Ship ship; public ShipArchetype archetype; public EnemyEntry(Ship s, ShipArchetype a) { ship = s; archetype = a; } }
    public List<EnemyEntry> enemies = new List<EnemyEntry>();

    Camera ScreenCamera;
    
    void Awake()
    {
        ScreenCamera = Camera.main;
        archetypes = new List<ShipArchetype>(){new ArchetypeWasp(),new ArchetypeFlagship(), new ArchetypePatrol()};//,new WaspArchetype() new FlagshipArchetype() new PatrolArchetype()
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
            //Debug.DrawLine(enemies[i].ship.transform.position, ScreenCamera.ViewportToWorldPoint(enemies[i].archetype.currentTarget), Color.green);
            //Debug.DrawLine(enemies[i].ship.transform.position, enemies[i].ship.transform.position + ScreenCamera.ViewportToWorldPoint(enemies[i].archetype.currentDirection), Color.blue);
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
                if (entry.archetype.state==EnemyState.Flee) DestroyImmediate(entry.ship.gameObject);
                else entry.archetype.OnOutOfBounds(entry.ship, this);
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
        
        ShipArchetype baseArchetype = archetypes[Random.Range(0, archetypes.Count)];
        ShipArchetype newArchetype = (ShipArchetype)System.Activator.CreateInstance(baseArchetype.GetType());
        var newShip = SFactory.GetShip(newArchetype.moduleWeights,GetModuleCount(newArchetype.minModules, newArchetype.maxModules));
        newShip.transform.SetParent(this.transform);
        newArchetype.controlledShip=newShip.GetComponent<Ship>();
        newArchetype.targetShip = playerShip;
        RegisterEnemy(newShip.GetComponent<Ship>(),newArchetype);
        
        newShip.transform.position = GetTopSpawnPosition(newArchetype.type);
        HUDConsole.EnqueueMessage("> UNKNOWN SIGNAL — CLASS: " + newArchetype.type.ToString().ToUpper());
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
        HUDConsole.EnqueueMessage("> HOSTILE SIGNAL LEFT THE SPACE");
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
    public Vector3 GetTopSpawnPosition(ShipArchetypeType shipType)
    {
        Vector3 topWorld = new Vector3(0.5f, 1.05f, 0);
        if (shipType==ShipArchetypeType.Flagship)
            topWorld = ScreenCamera.ViewportToWorldPoint(new Vector3(Random.Range(0.2f,0.8f), 1.05f, 0f));
        else if (shipType == ShipArchetypeType.Patrol || shipType == ShipArchetypeType.Wasp)
        {
            var randomX = Random.Range(0.1f, 0.25f);
            if (Random.Range(0,2)==0) randomX = Random.Range(0.75f, 0.9f);
            topWorld = ScreenCamera.ViewportToWorldPoint(new Vector3(randomX, 1.05f, 0f));
        }
        return new Vector3(topWorld.x, topWorld.y, 0);
    }
    int GetModuleCount(int minModules, int maxModules, float difficulty=1)
    {
        int scaledMin = Mathf.RoundToInt(minModules * difficulty);
        int scaledMax = Mathf.RoundToInt(maxModules * difficulty);

        return Random.Range(scaledMin, scaledMax + 1);
    }
}
