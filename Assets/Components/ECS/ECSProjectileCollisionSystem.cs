using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(ECSProjectileMovementSystem))]
public partial class ECSProjectileCollisionSystem : SystemBase
{
    private Collider2D[] hits;

    protected override void OnCreate()
    {
        hits = new Collider2D[16];
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, projectile, collision, entity) in
                 SystemAPI.Query<
                         RefRO<LocalTransform>,
                         RefRO<ECSProjectile>,
                         RefRO<ECSProjectileCollision>>()
                     .WithEntityAccess())
        {
            Vector2 position = new Vector2(
                transform.ValueRO.Position.x,
                transform.ValueRO.Position.y
            );

            int hitCount = Physics2D.OverlapCircleNonAlloc(
                position,
                collision.ValueRO.Radius,
                hits
            );

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null)
                    continue;

                DamageAdapter adapter = hit.GetComponent<DamageAdapter>();
                if (adapter == null)
                    continue;

                if (IsSameOwner(adapter, projectile.ValueRO.OwnerId))
                    continue;

                adapter.TakeDamage?.Invoke((int)projectile.ValueRO.Damage);

                ecb.DestroyEntity(entity);
                break;
            }
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    private bool IsSameOwner(DamageAdapter adapter, int projectileOwnerId)
    {
        if (adapter.owner == null)
            return false;

        Ship ownerShip = adapter.owner.GetComponent<Ship>();
        if (ownerShip == null)
            return false;

        return ownerShip.shipId == projectileOwnerId;
    }
}