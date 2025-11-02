using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
/*
public static class ECSProjectileManager
{
    public static Entity projectilePrefab;

    public static void Initialize(Entity prefab)
    {
        projectilePrefab = prefab;
    }

    public static void SpawnProjectile(EntityManager em, float2 pos, float2 dir, int damage, bool isMissile)
    {
        Entity e = em.Instantiate(projectilePrefab);
        em.SetComponentData(e, new ECSProjectile {
            position = pos,
            velocity = dir,
            damage = damage,
            lifetime = isMissile ? 5f : 2f,
            age = 0f,
            owner = Entity.Null,
            isMissile = isMissile
        });
        em.SetComponentData(e, new Translation { Value = new float3(pos.x, pos.y, 0f) });
    }
}
*/