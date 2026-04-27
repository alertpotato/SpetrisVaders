using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct ECSProjectileMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, projectile) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRO<ECSProjectile>>())
        {
            float2 move = projectile.ValueRO.Velocity * dt;
            transform.ValueRW.Position += new float3(move.x, move.y, 0f);
        }
    }
}