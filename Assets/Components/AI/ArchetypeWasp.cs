using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class ArchetypeWasp : ShipArchetype
{

    public float directionChangeChance = 0.5f;

    private float orbitRadius = 15f;           // Радиус орбиты
    private float orbitStepDegrees = 20f;     // Шаг по орбите
    private float currentAngle = 0f;          // Текущий угол на орбите
    private Vector2 orbitTarget;              // Текущая цель

    private int orbitDirection = 1;
    private float orbitRadiusRandomizer = 1f;
    private float orbitSpeed = 3f;

    public ArchetypeWasp()
    {
        type = ShipArchetypeType.Wasp;
        minModules = 2; maxModules = 4;
        stateDurationMin = 12f; stateDurationMax = 15f;

        moduleWeights[ModuleType.Canon] = 2;
        moduleWeights[ModuleType.Speed] = 5;
        moduleWeights[ModuleType.Missile] = 5;
        moduleWeights[ModuleType.PointDefense] = 5;
        moduleWeights[ModuleType.Shield] = 2;
        
        orbitRadiusRandomizer = Random.Range(0.8f, 1.5f);
        maxSpeed = 7;
        targetThreshold = 4;
        
        InitDefaults();
        ShipBuildPriority = new Dictionary<Vector2Int, float>() {
            { Vector2Int.up, 0.40f },
            { Vector2Int.down, 0.30f },
            { Vector2Int.left, 0.15f },
            { Vector2Int.right, 0.15f }
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
               if (Random.value < directionChangeChance)
                   orbitDirection *= -1;
               if (Random.value < directionChangeChance)
                   orbitRadiusRandomizer = Random.Range(0.8f, 1.5f);
               currentTarget = new Vector2(-999, -999);
               currentDirection = Vector2.zero;
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
       ship.FireMissle(new List<Ship>() { player });
       if (IsAlignedWithPlayer(ship, player, verticalAlignTolerance)) ship.FireCanons();
    }

    public bool UpdateDestination()
    {
       bool targetUpdated = false;
       if (state == EnemyState.Flee) {UpdateFleeDestination(); return true;}
       
       Vector2 targetPos = targetShip.transform.position;
       if (currentTarget.x==-999) UpdateCurrentAngle();
       if (currentTarget == new Vector2(-999, -999) || Vector2.Distance(controlledShip.transform.position, currentTarget) < targetThreshold)
       {
           currentAngle += orbitStepDegrees * orbitDirection;
           if (currentAngle > 360f) currentAngle -= 360f;

           
           float rad = currentAngle * Mathf.Deg2Rad;

           // Вычисляем новую точку на окружности вокруг цели
           var distanceBetweenShipsCorrection = Mathf.Clamp(controlledShip.DistanceToShip(targetShip)-controlledShip.DistanceToObject(targetShip.transform.position),0,20f);
           currentTarget = targetPos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * (orbitRadius * orbitRadiusRandomizer + distanceBetweenShipsCorrection);
           targetUpdated = true;
       }

       currentDirection = (currentTarget - (Vector2)controlledShip.transform.position).normalized;
       DrawOrbitDebug(targetPos,Vector2.Distance(currentTarget,targetPos), Color.cyan,18);
       return targetUpdated;
    }

    private void UpdateCurrentAngle()
    {
       Vector2 toShip = (controlledShip.transform.position - targetShip.transform.position).normalized;
       var orbitAngle = Mathf.Atan2(toShip.y, toShip.x) * Mathf.Rad2Deg;
       currentAngle = Mathf.Round(orbitAngle / 90f) * 90f;
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
        
        var distCoeff = Mathf.Clamp( distance+targetThreshold,0,16)/16;
        float desiredSpeed = Mathf.Lerp(orbitSpeed,maxSpeed, distCoeff);
        if (body.speed > desiredSpeed)
        {
            currentDirection = -body.velocity.normalized*0.1f;
        }
        else
        {
            currentDirection = GetCorrectedVelocity(currentVelDir,desiredDir,3f).normalized * Mathf.Lerp(0.5f,1, distCoeff);
        }
        //Debug.DrawLine(ship.transform.position, ship.transform.position+(Vector3)currentDirection*2, Color.red);
        //Debug.DrawLine(ship.transform.position, ship.transform.position+(Vector3)desiredDir*2, Color.green);
        
        return currentDirection;
    }

    public override void ChangeState(EnemyState newState)
   {
       state = newState;
   }
   public override void OnOutOfBounds(Ship ship, EnemyManager manager)
   {
       Vector3 vpTop = manager.GetTopSpawnPosition(type);
       ship.transform.position = vpTop;
       ship.inertialBody.velocity = Vector2.zero;
       ResetStateTimer();
       ChangeState(EnemyState.Traveling);
   }
   
   void DrawOrbitDebug(Vector2 center, float radius, Color color, int segments = 36)
   {
       float angleStep = 360f / segments;
       Vector3 prevPoint = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;

       for (int i = 1; i <= segments; i++)
       {
           float rad = Mathf.Deg2Rad * (i * angleStep);
           Vector3 nextPoint = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

           Debug.DrawLine(prevPoint, nextPoint, color);
           prevPoint = nextPoint;
       }
   }
}
