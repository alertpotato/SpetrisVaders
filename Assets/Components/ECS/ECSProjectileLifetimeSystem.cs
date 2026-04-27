using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct ECSProjectileLifetimeSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (projectile, entity) in 
                 SystemAPI.Query<RefRW<ECSProjectile>>()
                     .WithEntityAccess())
        {
            projectile.ValueRW.Lifetime -= deltaTime;

            if (projectile.ValueRO.Lifetime <= 0f || projectile.ValueRO.Health <= 0f)
            {
                ecb.DestroyEntity(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}