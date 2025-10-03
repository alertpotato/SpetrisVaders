using System.Collections.Generic;
using UnityEngine;
/*
public class ScoutArchetype : ShipArchetype
{
    public ScoutArchetype()
    {
        type = ShipArchetypeType.Scout;
        minModules = 1; maxModules = 2;
        fireCooldownMin = 1.2f; fireCooldownMax = 2.5f;
        stateDurationMin = 3f; stateDurationMax = 6f;

        moduleWeights[ModuleType.Canon] = 1;
        moduleWeights[ModuleType.Speed] = 4;
        moduleWeights[ModuleType.Missile] = 3;
        moduleWeights[ModuleType.PointDefense] = 4;
        moduleWeights[ModuleType.Shield] = 1;

        InitDefaults();
    }

    public override void UpdateBehavior(Ship ship, Ship player, float dt, EnemyManager manager)
    {
        // поведение зависит от state
        if (state == EnemyState.Traveling)
            ship.inertialBody.ApplyForce(Vector2.down * ship.thrust, dt);
        else if (state == EnemyState.Retreating)
            ship.inertialBody.ApplyForce(Vector2.up * (ship.thrust * 0.6f), dt);
        // автоматически стреляем в Tick через TryAutoShoot()
    }

    public override void OnOutOfBounds(Ship ship, EnemyManager manager)
    {
        // разведчик просто деспавнится
        GameObject.Destroy(ship.gameObject);
    }
}

public class PatrolArchetype : ShipArchetype
{
    private float patrolSpeedFactor = 0.6f;
    public PatrolArchetype()
    {
        type = ShipArchetypeType.Patrol;
        minModules = 2; maxModules = 4;
        fireCooldownMin = 1.5f; fireCooldownMax = 3.0f;
        stateDurationMin = 4f; stateDurationMax = 8f;
        moduleWeights[ModuleType.Canon] = 5;
        moduleWeights[ModuleType.Speed] = 2;
        moduleWeights[ModuleType.Missile] = 1;
        moduleWeights[ModuleType.PointDefense] = 3;
        moduleWeights[ModuleType.Shield] = 4;
        InitDefaults();
    }

    public override void UpdateBehavior(Ship ship, Ship player, float dt, EnemyManager manager)
    {
        // простая патрульная качалка в X + небольшая вертикальная стабилизация
        float t = Time.time * 0.6f;
        Vector2 dir = new Vector2(Mathf.Sin(t), Mathf.Cos(t) * 0.1f).normalized;
        ship.inertialBody.ApplyForce(dir * ship.thrust * patrolSpeedFactor, dt);
    }

    public override void OnOutOfBounds(Ship ship, EnemyManager manager)
    {
        // телепортируем его обратно наверх с той же X
        Vector3 vpTop = manager.GetTopSpawnPositionFor(ship);
        ship.transform.position = vpTop;
        ship.inertialBody.velocity = Vector2.zero;
    }
}

public class RamArchetype : ShipArchetype
{
    public RamArchetype()
    {
        type = ShipArchetypeType.Ram;
        minModules = 2; maxModules = 5;
        fireCooldownMin = 0.8f; fireCooldownMax = 2f;
        stateDurationMin = 3f; stateDurationMax = 5f;
        InitDefaults();
    }

    public override void UpdateBehavior(Ship ship, Ship player, float dt, EnemyManager manager)
    {
        if (player == null) return;
        Vector2 dir = ((Vector2)player.transform.position - (Vector2)ship.transform.position).normalized;
        ship.inertialBody.ApplyForce(dir * ship.thrust, dt);
    }

    public override void OnOutOfBounds(Ship ship, EnemyManager manager)
    {
        // просто деспавн
        GameObject.Destroy(ship.gameObject);
    }
}

public class WaspArchetype : ShipArchetype
{
    public float keepDistanceMin = 4f;
    public float keepDistanceMax = 7f;

    public WaspArchetype()
    {
        type = ShipArchetypeType.Wasp;
        minModules = 1; maxModules = 3;
        fireCooldownMin = 1.0f; fireCooldownMax = 2.2f;
        stateDurationMin = 2f; stateDurationMax = 5f;
        InitDefaults();
    }

    public override void UpdateBehavior(Ship ship, Ship player, float dt, EnemyManager manager)
    {
        if (player == null) return;
        float dist = DistanceToPlayer(ship, player);
        Vector2 dir = ((Vector2)player.transform.position - (Vector2)ship.transform.position).normalized;
        if (dist > keepDistanceMax)
            ship.inertialBody.ApplyForce(dir * ship.thrust, dt);
        else if (dist < keepDistanceMin)
            ship.inertialBody.ApplyForce(-dir * ship.thrust, dt);
        else
            ship.inertialBody.ApplyForce(Vector2.zero, dt);
    }

    public override void OnOutOfBounds(Ship ship, EnemyManager manager)
    {
        // телепорт на верх экрана (сохраняя X)
        Vector3 vpTop = manager.GetTopSpawnPositionFor(ship);
        ship.transform.position = vpTop;
        ship.inertialBody.velocity = Vector2.zero;
    }
}
*/
public class FlagshipArchetype : ShipArchetype
{
    public FlagshipArchetype()
    {
        type = ShipArchetypeType.Flagship;
        minModules = 3; maxModules = 5;
        stateDurationMin = 1f; stateDurationMax = 3f;
        moduleWeights[ModuleType.Canon] = 6;
        moduleWeights[ModuleType.Speed] = 2;
        moduleWeights[ModuleType.Missile] = 1;
        moduleWeights[ModuleType.PointDefense] = 4;
        moduleWeights[ModuleType.Shield] = 5;
        InitDefaults();
    }

    public override void OnStateTimeout(Ship ship, Ship player)
    {
        switch (state)
        {
            case EnemyState.Idle:
                if (!IsAlignedWithPlayer(ship, player, verticalAlignTolerance))
                    state = EnemyState.Traveling;
                //Chance to break idle and move away
                else if (Random.value < 0.3f)
                {
                    state = EnemyState.Retreating;
                }
                break;
            
            case EnemyState.Traveling:
                if (IsAlignedWithPlayer(ship, player, verticalAlignTolerance)) state = EnemyState.Idle;
                break;

            case EnemyState.Retreating:
                state = EnemyState.Traveling;
                break;
        }
    }
    public override void TryToFire(Ship ship, Ship player)
    {
        switch (state)
        {
            case EnemyState.Idle:
                ship.FireCanons();
                ship.FireMissle();
                break;
            
            case EnemyState.Traveling:
                ship.FireMissle();
                if (IsAlignedWithPlayer(ship, player, verticalAlignTolerance / 2)) ship.FireCanons();;
                break;

            case EnemyState.Retreating:
                break;
        }
    }

    public override bool GetDestination(Vector3 shipViewportPos, Vector3 playerViewportPos, out Vector2 direction)
    {
        direction = Vector2.zero;
        currentDirection = direction;
        if (state == EnemyState.Traveling)
        {
            Vector3 target = new Vector3(playerViewportPos.x,
                Random.Range(Mathf.Clamp(0.7f, playerViewportPos.y, 0.85f), 0.9f), 0f);
            direction = ((Vector2)target - (Vector2)shipViewportPos);
            currentDirection = direction;
            return true;
        }
        if (state == EnemyState.Retreating)
        {
            Vector3 target = new Vector3(Random.Range(playerViewportPos.x-0.3f,playerViewportPos.x+0.3f),
                Random.Range(Mathf.Clamp(0.7f, playerViewportPos.y, 0.85f), 0.9f), 0f);
            direction = ((Vector2)target - (Vector2)shipViewportPos);
            currentDirection = direction;
            return true;
        }
        else return false;
    }

    public override void OnOutOfBounds(Ship ship, EnemyManager manager)
    {
        Vector3 vpTop = manager.GetTopSpawnPositionFor(ship);
        ship.transform.position = vpTop;
        ship.inertialBody.velocity = Vector2.zero;
    }
}
