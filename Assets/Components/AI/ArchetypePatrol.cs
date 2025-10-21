using System.Collections.Generic;
using UnityEngine;


public class ArchetypePatrol : ShipArchetype
{
    float patrolDistance = 10;
    public ArchetypePatrol()
    {
        type = ShipArchetypeType.Patrol;
        minModules = 2; maxModules = 5;
        stateDurationMin = 3f; stateDurationMax = 6f;
        
        moduleWeights[ModuleType.Canon] = 3;
        moduleWeights[ModuleType.Speed] = 3;
        moduleWeights[ModuleType.Missile] = 6;
        moduleWeights[ModuleType.PointDefense] =5;
        moduleWeights[ModuleType.Shield] = 5;

        maxSpeed = 3;
        targetThreshold = 2;
            
        InitDefaults();
        ShipBuildPriority = new Dictionary<Vector2Int, float>() {
            { Vector2Int.up, 0.20f },
            { Vector2Int.down, 0.20f },
            { Vector2Int.left, 0.30f },
            { Vector2Int.right, 0.30f }
        };
    }

    public override void OnStateTimeout(Ship ship, Ship player)
    {
        switch (state)
        {
            case EnemyState.Idle:
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
            case EnemyState.Flee:
                break;
        }
    }

    public override Vector2 GetVelocity(InertialBody body, Vector3 shipViewportPos, Vector3 playerViewportPos)
    {
        UpdateDestination();

        Vector2 toTarget = (currentTarget - (Vector2)controlledShip.transform.position);
        float distance = toTarget.magnitude;

        Vector2 desiredDir = (distance > 0.0001f) ? toTarget.normalized : currentDirection;
        Vector2 currentVelDir = (body.velocity.sqrMagnitude > 1e-5f)
            ? body.velocity.normalized
            : desiredDir;
        
        var distCoeff = Mathf.Clamp( distance,0,10)/10;
        float desiredSpeed = Mathf.Lerp(maxSpeed*0.1f,maxSpeed*2, distCoeff);
        if (body.speed > desiredSpeed)
        {
            currentDirection = -body.velocity.normalized*0.1f;
        }
        else
        {
            currentDirection = GetCorrectedVelocity(currentVelDir,desiredDir,3f).normalized * Mathf.Lerp(0.5f,1, distCoeff);
        }
        Debug.DrawLine(controlledShip.transform.position, currentTarget, Color.blue);
        Debug.DrawLine(controlledShip.transform.position, controlledShip.transform.position+(Vector3)currentDirection*2, Color.red);
        Debug.DrawLine(controlledShip.transform.position, controlledShip.transform.position+(Vector3)desiredDir*2, Color.green);
        
        return currentDirection;
    }

    public bool UpdateDestination()
    {
        bool targetUpdated = false;
        if (state == EnemyState.Traveling)
        {
            if (currentTarget == new Vector2(-999, -999) ||
                Vector2.Distance(controlledShip.transform.position, currentTarget) < targetThreshold)
            {
                currentTarget = new Vector2(controlledShip.transform.position.x,
                    controlledShip.transform.position.y - patrolDistance);
                targetUpdated = true;
            }

            currentDirection = (currentTarget - (Vector2)controlledShip.transform.position).normalized;
        }
        else if (state == EnemyState.Flee) {UpdateFleeDestination(); targetUpdated = true;}
        else
        {
            currentDirection = Vector2.zero;
            currentTarget = controlledShip.transform.position;
            targetUpdated = false;
        }
        return targetUpdated;
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
