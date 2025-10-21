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
*/

public class ArchetypeFlagship : ShipArchetype
{
    public ArchetypeFlagship()
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
        ShipBuildPriority = new Dictionary<Vector2Int, float>() {
            { Vector2Int.up, 0.34f },
            { Vector2Int.down, 0.20f },
            { Vector2Int.left, 0.23f },
            { Vector2Int.right, 0.23f }
        };
    }

    public override void OnStateTimeout(Ship ship, Ship player)
    {
        switch (state)
        {
            case EnemyState.Idle:
                //Chance to break idle and move away
                if (Random.value < 0.3f) ChangeState(EnemyState.Regroup);
                if (!IsAlignedWithPlayer(ship, player, verticalAlignTolerance))
                    ChangeState(EnemyState.Traveling);
                break;
            case EnemyState.Traveling:
                if (IsAlignedWithPlayer(ship, player, verticalAlignTolerance)) ChangeState(EnemyState.Idle);
                break;
            case EnemyState.Regroup:
                ChangeState(EnemyState.Traveling);
                break;
            case EnemyState.Flee:
                ChangeState(EnemyState.Flee);
                break;
        }
    }
    public override void TryToFire(Ship ship, Ship player)
    {
        switch (state)
        {
            case EnemyState.Idle:
                ship.FireCanons();
                ship.FireMissle(player.transform.position);
                break;
            
            case EnemyState.Traveling:
                ship.FireMissle(player.transform.position);
                if (IsAlignedWithPlayer(ship, player, verticalAlignTolerance / 2)) ship.FireCanons();;
                break;

            case EnemyState.Regroup:
                break;
        }
    }

    public override Vector2 GetVelocity(InertialBody shipPhysics, Vector3 shipViewportPos, Vector3 playerViewportPos)
    {
        Vector2 normalizedVelocity = currentDirection;
        float virtualMaxSpeed = 10;
        UpdateDestination(shipViewportPos, playerViewportPos);
        //if too close
        if (Vector3.Distance(shipViewportPos, currentTarget) < 0.05f)
        {
            currentDirection = Vector2.zero; return currentDirection;}
        
        var maxSpeedCorrection = Mathf.Clamp(Vector2.Distance(shipViewportPos,playerViewportPos),0.1f,0.8f);
        if (maxSpeedCorrection > 0.5f) maxSpeedCorrection = 1;
        if (Vector2.Dot(currentDirection, shipPhysics.velocity) < 0) normalizedVelocity = -shipPhysics.velocity.normalized;
        else if (shipPhysics.speed > virtualMaxSpeed * maxSpeedCorrection) normalizedVelocity = -normalizedVelocity*maxSpeedCorrection;
        return normalizedVelocity;
    }

    public bool UpdateDestination(Vector3 shipViewportPos, Vector3 playerViewportPos)
    {
        if (state == EnemyState.Traveling)
        {
            //Direction correction
            currentDirection = ((Vector2)currentTarget - (Vector2)shipViewportPos).normalized;
            //Change target if old one is too far
            if (Mathf.Abs(currentTarget.x - playerViewportPos.x)<0.1f || shipViewportPos.y - playerViewportPos.y<-0.05f) return true;
            currentTarget = new Vector2(playerViewportPos.x,
                Random.Range(Mathf.Clamp(0.7f, playerViewportPos.y, 0.85f), 0.9f)) + Random.insideUnitCircle * 0.1f;
            currentDirection = ((Vector2)currentTarget - (Vector2)shipViewportPos).normalized;
            return true;
        }
        else if (state == EnemyState.Regroup)
        {
            if (currentDirection != Vector2.zero) return false;
            currentTarget = new Vector2(Random.Range(playerViewportPos.x - 0.3f, playerViewportPos.x + 0.3f),
                Random.Range(Mathf.Clamp(0.7f, playerViewportPos.y, 0.85f), 0.9f));
            currentDirection = ((Vector2)currentTarget - (Vector2)shipViewportPos).normalized;
            return true;
        }
        else if (state == EnemyState.Flee) {UpdateFleeDestination();return true;}
        else
        {
            currentDirection = Vector2.zero;
            currentTarget = shipViewportPos;
            return false;
        }
    }

    public override void ChangeState(EnemyState newState)
    {
        state = newState;
        currentTarget = new Vector2(-999, -999);
        currentDirection = Vector2.zero;
    }

    public override void OnOutOfBounds(Ship ship, EnemyManager manager)
    {
        Vector3 vpTop = manager.GetTopSpawnPosition(type);
        ship.transform.position = vpTop;
        ship.inertialBody.velocity = Vector2.zero;
        ResetStateTimer();
        ChangeState(EnemyState.Traveling);
    }
}
