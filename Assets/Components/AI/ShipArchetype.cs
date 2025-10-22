using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyState { Traveling, Idle, Regroup, Flee }
public enum ShipArchetypeType { Scout, Patrol, Ram, Wasp, Flagship }

public abstract class ShipArchetype
{
    public ShipArchetypeType type;
    public Ship controlledShip;
    public Ship targetShip;
    public Dictionary<ModuleType, int> moduleWeights = new Dictionary<ModuleType, int>();
    public int minModules = 1;
    public int maxModules = 3;
    // ship control parameters
    public float maxSpeed = 10f;
    public float targetThreshold=10f;
    // state machine
    public EnemyState state = EnemyState.Idle;
    public float stateTimer = 0f;
    public float stateDurationMin = 5f;
    public float stateDurationMax = 10f;
    
    public float verticalAlignTolerance = 5f;
    public Vector2 currentDirection = Vector2.zero;
    public Vector2 currentTarget = new Vector2(-999, -999);
    private List<ModuleType> weaponModules;
    public Dictionary<Vector2Int, float> ShipBuildPriority;
    
    
    protected void InitDefaults()
    {
        if (moduleWeights == null) moduleWeights = new Dictionary<ModuleType, int>();
        if (moduleWeights.Count == 0)
        {
            foreach (ModuleType m in System.Enum.GetValues(typeof(ModuleType)))
                moduleWeights[m] = 1;
        }
        currentDirection = Vector2.zero;
        currentTarget = new Vector2(-999, -999);
        weaponModules = new List<ModuleType>() { ModuleType.Canon, ModuleType.Missile, ModuleType.PointDefense };
    }

    protected void ResetStateTimer()
    {
        stateTimer = Random.Range(stateDurationMin, stateDurationMax);
    }
    
    public void Tick(Ship ship, Ship player, float dt, EnemyManager manager)
    {
        if (ship == null || player==null) return;

        stateTimer -= dt;
        if (stateTimer <= 0f)
        {
            if (!CheckForCombatReadiness())
            {
                ChangeState(EnemyState.Flee);
                stateTimer = 999;
                return;
            }

            OnStateTimeout(ship, player);
            ResetStateTimer();
        }
        
        TryToFire(ship, player);
    }

    public abstract void OnStateTimeout(Ship ship, Ship player);
    public abstract void TryToFire(Ship ship, Ship player);
    public abstract Vector2 GetVelocity(InertialBody shipPhysics, Vector3 shipViewportPos, Vector3 playerViewportPos);
    public abstract void ChangeState(EnemyState newState);
    public Vector2 GetCorrectedVelocity(Vector2 currentVelocity, Vector2 desiredVelocity, float velocityCorrectionMagnitude)
    {
        Vector2 corrected=desiredVelocity;
        float alignment = Vector2.Dot(currentVelocity, desiredVelocity);
        Vector2 counterVelocity = -currentVelocity * Mathf.Clamp01(1f - alignment);
        corrected = desiredVelocity + counterVelocity * velocityCorrectionMagnitude;
        return corrected;
    }
    protected bool IsAlignedWithPlayer(Ship ship, Ship player, float tolerance)
    {
        return Mathf.Abs(ship.transform.position.x - player.transform.position.x) <= tolerance;
    }
    public bool CheckForCombatReadiness()
    {
        bool ready = false;
        int weaponCount = 0;
        foreach (var module in controlledShip.modules.Where(x=>x.isFunctioning))
        {
            if (weaponModules.Contains(module.data.type)) ready=true;
        }
        return ready;
    }
    public void UpdateFleeDestination()
    {
        currentTarget = new Vector2(controlledShip.transform.position.x, -999);
        currentDirection = (currentTarget - (Vector2)controlledShip.transform.position).normalized;
    }

    protected float DistanceToPlayer(Ship ship, Ship player)
    {
        if (ship == null || player == null) return float.MaxValue;
        return Vector2.Distance(ship.transform.position, player.transform.position);
    }
    public abstract void OnOutOfBounds(Ship ship, EnemyManager manager);
}
