using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { Traveling, Idle, Retreating }
public enum ShipArchetypeType { Scout, Patrol, Ram, Wasp, Flagship }

public abstract class ShipArchetype
{
    public ShipArchetypeType type;
    public Dictionary<ModuleType, int> moduleWeights = new Dictionary<ModuleType, int>();
    public int minModules = 1;
    public int maxModules = 3;

    // state machine
    public EnemyState state = EnemyState.Idle;
    public float stateTimer = 0f;
    public float stateDurationMin = 5f;
    public float stateDurationMax = 10f;
    
    public float verticalAlignTolerance = 5f;
    public Vector2 currentDirection = Vector2.zero;
    
    protected void InitDefaults()
    {
        if (moduleWeights == null) moduleWeights = new Dictionary<ModuleType, int>();
        if (moduleWeights.Count == 0)
        {
            foreach (ModuleType m in System.Enum.GetValues(typeof(ModuleType)))
                moduleWeights[m] = 1;
        }
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
            OnStateTimeout(ship, player);
            ResetStateTimer();
        }
        
        TryToFire(ship, player);
    }

    public abstract void OnStateTimeout(Ship ship, Ship player);
    public abstract void TryToFire(Ship ship, Ship player);
    public abstract bool GetDestination(Vector3 shipViewportPos,Vector3 playerViewportPos, out Vector2 direction);

    protected bool IsAlignedWithPlayer(Ship ship, Ship player, float tolerance)
    {
        return Mathf.Abs(ship.transform.position.x - player.transform.position.x) <= tolerance;
    }

    protected float DistanceToPlayer(Ship ship, Ship player)
    {
        if (ship == null || player == null) return float.MaxValue;
        return Vector2.Distance(ship.transform.position, player.transform.position);
    }
    public abstract void OnOutOfBounds(Ship ship, EnemyManager manager);
}
