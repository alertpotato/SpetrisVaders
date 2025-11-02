using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ECSProjectile : IComponentData
{
    public float2 position;
    public float2 velocity;
    public float lifetime;
    public float age;
    public int damage;
    public Entity owner;
    public bool isMissile;
}

public struct Collider2DData : IComponentData
{
    public float radius;
    public LayerMask hitMask;
}
public struct Sprite2DEntity : IComponentData
{
    public int spriteIndex;       // например индекс в атласе или ID материала
    public float4 color;
    // возможно другие параметры: flip, scale, rotation
}
