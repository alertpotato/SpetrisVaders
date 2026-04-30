using Unity.Entities;
using UnityEngine;

public class ProjectileBenchmark : MonoBehaviour
{
    public enum Mode
    {
        OldGameObject,
        ECS
    }

    [Header("Test")]
    public Mode mode = Mode.ECS;
    public int projectileCount = 10000;
    public float spawnRadius = 5f;
    public int damage = 1;
    public GameObject owner;

    [Header("Runtime")]
    public int aliveOld;
    public int aliveECS;
    public float fps;
    
    public ShipFactory SFactory;
    public EnemyManager EManager;

    private EntityQuery ecsProjectileQuery;

    private void Start()
    {
        ecsProjectileQuery = World.DefaultGameObjectInjectionWorld
            .EntityManager
            .CreateEntityQuery(typeof(ECSProjectile));
        owner = CreateTestShip();
    }
    public GameObject CreateTestShip()
    {
        var flagship = SFactory.GetShip(new ArchetypeFlagship().moduleWeights, 20, faction: Faction.Player,
            shipAlignment: 0, directionChances: new ArchetypeFlagship().ShipBuildPriority);
        flagship.transform.localPosition = new Vector3(-100, -100, -100);
        flagship.transform.SetParent(transform);
        return flagship;
    }

    private void Update()
    {
        fps = 1f / Time.unscaledDeltaTime;

        aliveOld = ProjectileManager.Instance.activeProjectiles.Count;
        aliveECS = ecsProjectileQuery.CalculateEntityCount();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mode = Mode.OldGameObject;
            SpawnBatch();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mode = Mode.ECS;
            SpawnBatch();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnEnemies();
        }
    }

    private void SpawnBatch()
    {
        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * spawnRadius);

            if (mode == Mode.OldGameObject)
            {
                ProjectileManager.Instance.SpawnShell(pos, dir, damage, owner);
            }
            else
            {
                ProjectileManager.Instance.SpawnECSShell(pos, dir, damage, owner);
            }
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < 50; i++)
        {
            EManager.SpawnShip();
        }

        foreach (var enemy in EManager.enemies)
        {
            enemy.ship.transform.position = (Vector3)(Random.insideUnitCircle * 50);
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 300, 180), GUI.skin.box);

        GUILayout.Label($"Mode: {mode}");
        GUILayout.Label($"FPS: {fps:F1}");
        GUILayout.Label($"Old projectiles alive: {aliveOld}");
        GUILayout.Label($"ECS projectiles alive: {aliveECS}");
        GUILayout.Label($"Spawn count: {projectileCount}");
        GUILayout.Label("Press 1: Spawn old GameObject shells");
        GUILayout.Label("Press 2: Spawn ECS shells");

        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        ecsProjectileQuery.Dispose();
    }
}