using UnityEngine;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine.Rendering;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;

    public GameObject missilePrefab;
    public GameObject shellPrefab;
    
    [SerializeField]private GameObject shellsParent;
    [SerializeField]private GameObject missileParent;
    
    [SerializeField]private ParticleSystem impactDirected;
    [SerializeField]private ParticleSystem bullets;
    [SerializeField]private ParticleSystem impactMetal;
    
    public List<Projectile> activeProjectiles = new List<Projectile>();
    
    [Header("ECS Projectiles")]
    private EntityManager entityManager;
    private Entity ecsShellPrefab;
    private bool ecsReady;
    [SerializeField] private float ecsShellSpeed = 20f;
    [SerializeField] private float ecsShellLifetime = 15f;
    [SerializeField] private float ecsShellHealth = 1f;
    [SerializeField] private float ecsShellRadius = 0.15f;
    [Header("ECS Projectile Graphics")]
    [SerializeField] private Mesh ecsShellMesh;
    [SerializeField] private Material ecsShellMaterial;
    void Awake()
    {
        Instance = this;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CreateECSShellPrefab();
    }

    void Update()
    {
        activeProjectiles.RemoveAll(p => p == null);
    }

    public void SpawnMissile(Vector3 spawnPos, Vector2 targetPos,Vector3 startDirection,int damage, GameObject owner)
    {
        GameObject missileObj = Instantiate(missilePrefab, spawnPos, Quaternion.identity);
        MissileProjectile missile = missileObj.GetComponent<MissileProjectile>();
        missile.Launch(startDirection, targetPos,damage, owner);
        missile.transform.SetParent(missileParent.transform);
        activeProjectiles.Add(missile);
    }

    public void SpawnShell(Vector3 spawnPos, Vector2 direction,int damage, GameObject owner)
    {
        GameObject shellObj = Instantiate(shellPrefab, spawnPos, Quaternion.identity);
        ShellProjectile shell = shellObj.GetComponent<ShellProjectile>();
        shell.Launch(direction, Vector2.zero,damage, owner);
        shell.transform.SetParent(shellsParent.transform);
        activeProjectiles.Add(shell);
    }
    public void SpawnECSShell(Vector3 spawnPos, Vector2 direction, int damage, GameObject owner)
    {
        if (!ecsReady || ecsShellPrefab == Entity.Null)
        {
            Debug.LogWarning("ECS shell prefab is not ready.");
            return;
        }

        Entity shellEntity = entityManager.Instantiate(ecsShellPrefab);

        Vector2 velocity = direction.normalized * ecsShellSpeed;
        Vector2 dir = direction.normalized;
        float angle = math.atan2(dir.y, dir.x);
        quaternion rotation = quaternion.RotateZ(angle - math.PI / 2f);//sprite rotation
        
        int ownerId = -1;
        Ship ownerShip = owner.GetComponent<Ship>();
        if (ownerShip != null)
            ownerId = ownerShip.shipId;

        entityManager.SetComponentData(shellEntity, LocalTransform.FromPosition(
            new float3(spawnPos.x, spawnPos.y, spawnPos.z)
        ));
        entityManager.SetComponentData(shellEntity,
            LocalTransform.FromPositionRotationScale(
                new float3(spawnPos.x, spawnPos.y, spawnPos.z),
                rotation,
                1
            )
        );

        entityManager.SetComponentData(shellEntity, new ECSProjectile
        {
            Health = ecsShellHealth,
            Damage = damage,
            Lifetime = ecsShellLifetime,
            OwnerId = ownerId,
            Velocity = new float2(velocity.x, velocity.y)
        });
    }

    public void SpawnPointDefenseShot(Vector3 from, Vector3 to, int damage, float range,float spread, GameObject owner)
    {
        SpawnBulletEffect(from, to,  spread, range,owner.transform);
        Vector2 dir = (to - from).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(from, dir, range);
        foreach (var hit in hits)
        {
            var adapter = hit.collider.GetComponent<DamageAdapter>();
            if (adapter != null && adapter.owner != owner)
            {
                adapter.TakeDamage?.Invoke(damage);
                Debug.DrawLine(from, hit.point, Color.blue, 5f);
                return;
            }
        }
        //Debug.DrawLine(from, from + (Vector3)dir * range, Color.red, 5f);
    }
    public void SpawnBulletEffect(Vector3 from, Vector3 to,float radiusSpread, float maxRange,Transform origin)
    {
        //direction and arc based on bullet accuracy
        var direction = to - from;
        float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float arc = Mathf.Rad2Deg * 2f * Mathf.Atan(radiusSpread / maxRange);
        ParticleSystem bullet = Instantiate(bullets,from,Quaternion.Euler(0f, 0f, angleZ-arc/2));
        var shape = bullet.shape;
        shape.arc = arc;
        //lifetime based on range and speed
        var main = bullet.main;
        main.startLifetime = maxRange / main.startSpeed.constant;
        //ignoring its own collision
        var collision = bullet.collision;
        collision.collidesWith = LayerMask.GetMask("PlayerShip", "EnemyShip", "Environment", "Projectile");
        int ownerLayer = origin.gameObject.layer;
        collision.collidesWith &= ~(1 << ownerLayer);
        
        bullet.transform.SetParent(origin);
    }

    public void SpawnImpactEffect(Vector3 position,Vector2 direction)
    {
        float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ParticleSystem impactM = Instantiate(impactMetal,position,Quaternion.identity);
    }
    
    private void CreateECSShellPrefab()
    {
        ecsShellPrefab = entityManager.CreateEntity(
            typeof(ECSProjectile),
            typeof(ECSProjectileCollision),
            typeof(LocalTransform)
        );

        entityManager.AddComponent<Prefab>(ecsShellPrefab);

        var renderMeshArray = new RenderMeshArray(
            new Material[] { ecsShellMaterial },
            new Mesh[] { ecsShellMesh }
        );

        var renderMeshDescription = new RenderMeshDescription(
            shadowCastingMode: ShadowCastingMode.Off,
            receiveShadows: false
        );
        
        entityManager.SetComponentData(ecsShellPrefab, new ECSProjectileCollision
        {
            Radius = ecsShellRadius
        });

        RenderMeshUtility.AddComponents(
            ecsShellPrefab,
            entityManager,
            renderMeshDescription,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
        );

        entityManager.SetComponentData(ecsShellPrefab, new ECSProjectile
        {
            Health = ecsShellHealth,
            Damage = 1,
            Lifetime = ecsShellLifetime,
            OwnerId = -1,
            Velocity = float2.zero
        });

        entityManager.SetComponentData(
            ecsShellPrefab,
            LocalTransform.FromPosition(float3.zero)
        );

        ecsReady = true;

        Debug.Log("Runtime ECS shell prefab with graphics created");
    }
}