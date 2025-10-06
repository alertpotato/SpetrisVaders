using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public enum Faction { Player, EvilFleet, Neutral }

[RequireComponent(typeof(ShipGrid))]
[RequireComponent(typeof(InertialBody))]
public class Ship : MonoBehaviour
{
    [Header("Components")]
    public InertialBody inertialBody;
    public ShipGrid grid;
    public List<ShipModule> modules = new List<ShipModule>();
    [SerializeField]private GameObject ModuleParent;
    public ShipModule cockpit;
    [Header("Ship stats")] 
    public int shipAlignment = 180;
    public float thrust = 10;
    public float maxSpeed = 10;
    public Faction faction;
    [Header("Weapon settings")]
    public float canonFireCooldown = 0.1f;
    public float missileFireCooldown = 0.2f;
    public float lastShot;

    public void InitializeShip(Faction shipFaction)
    { 
        faction = shipFaction;
    }

    private void UpdateStats()
    {
        maxSpeed = 10f;
        thrust = 10f;
        var colliders = new List<PolygonCollider2D>();
        foreach (var module in modules)
        {
            thrust += module.speedBonus;
            thrust += 2;
            colliders.Add(module.polyCollider);
        }
        
        inertialBody.UpdateBody(modules.Count,maxSpeed,colliders);
    }

    public void AttachModule(Candidate candidateModule)
    {
        var module = candidateModule.module.GetComponent<ShipModule>();
        var anchor = candidateModule.Primary.anchor;
        var adjustment = candidateModule.Primary.adjustment;
    
        grid.Attach(module, anchor, adjustment);
        module.transform.SetParent(ModuleParent.transform);
        module.transform.localPosition = new Vector3(anchor.x + adjustment.x, anchor.y + adjustment.y, 0);
        module.OnAttachToShip(this.gameObject, inertialBody, shipAlignment);
        modules.Add(module);
        
        if (cockpit == null)
            cockpit = module;

        UpdateStats();
    }

    public void OnModuleDestroyed(ShipModule module)
    {
        grid.RemoveModule(module);
        modules.Remove(module);
        var disconnectedModules = grid.GetDisconnectedModules(cockpit);
        Vector3 shipCenter = grid.GridToWorld(GetGridCenterLocal());
        
        foreach (var m in disconnectedModules)
        {
            modules.Remove(m);
            grid.RemoveModule(m);
            m.OnDetachFromShip(shipCenter);
        }
        
        if (module == cockpit)
        {
            Destroy(gameObject);
            return;
        }
        UpdateStats();
    }


    public bool FireCanons()
    {
        if (Time.time - lastShot < canonFireCooldown) return false;
        lastShot = Time.time;
        
        var direction = Vector3.up;
        if (shipAlignment != 0) direction = Vector3.down;
        foreach (var module in modules.Where(x=>x.data.type==ModuleType.Canon))
        {
            if (module.FireCanon(direction,this.GameObject())) return true;
        }
        return false;
    }
    public bool FireMissle(List<Ship> possibleTargets)
    {
        if (Time.time - lastShot < missileFireCooldown) return false;
        lastShot = Time.time;
        
        foreach (var module in modules.Where(x=>x.data.type==ModuleType.Missile))
        {
            if (module.FireMissile( possibleTargets,this.GameObject())) return true;
        }
        return false;
    }
    public Vector3 GetGridCenterLocal()
    {
        if (modules.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var m in modules)
        {
            sum += m.transform.localPosition;
        }
        return sum / modules.Count;
    }
}