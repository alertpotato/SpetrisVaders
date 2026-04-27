using Unity.Entities;
using Unity.Mathematics;

public struct ECSProjectile : IComponentData
{
    public float Health;
    public float Damage;
    public float Lifetime;

    public int OwnerId;

    public float2 Velocity;
}