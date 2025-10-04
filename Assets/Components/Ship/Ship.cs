using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

[RequireComponent(typeof(ShipGrid))]
[RequireComponent(typeof(InertialBody))]
public class Ship : MonoBehaviour
{
    [Header("Components")]
    public InertialBody inertialBody;
    public ShipGrid grid;
    public List<ShipModule> modules = new List<ShipModule>();
    [SerializeField]private GameObject ModuleParent;
    [Header("Ship stats")] 
    public int shipAlignment = 180;
    public float thrust = 10;
    public float maxSpeed = 10;
    
    private void UpdateStats()
    {
        maxSpeed = 10f;
        thrust = 10f;
        var colliders = new List<PolygonCollider2D>();
        foreach (var module in modules)
        {
            thrust += module.speedBonus;
            thrust += 5;
            colliders.Add(module.polyCollider);
        }
        
        inertialBody.UpdateBody(modules.Count,maxSpeed,colliders);
    }

    public void AttachModule(Candidate candidateModule)
    {
        var anchor = candidateModule.Primary.anchor;
        var adjustment = candidateModule.Primary.adjustment;
        grid.Attach(candidateModule.module.GetComponent<ShipModule>(), anchor, adjustment);
        candidateModule.module.transform.SetParent(ModuleParent.transform);
        candidateModule.module.transform.localPosition =
            new Vector3(anchor.x + adjustment.x, anchor.y + adjustment.y, 0);
        candidateModule.module.GetComponent<ShipModule>().OnAttachToShip(this.GameObject(),inertialBody, shipAlignment);
        modules.Add(candidateModule.module.GetComponent<ShipModule>());
        UpdateStats();
    }
    public void OnModuleDamaged(float damage)
    {
        Debug.Log($"Ship {name} получил {damage}");
    }
    public void OnModuleDestroyed(ShipModule module)
    {
        grid.RemoveModule(module);
        modules.Remove(module);
        Debug.Log($"Module {module.name} destroyed");
    }

    public bool FireCanons()
    {
        var direction = Vector3.up;
        if (shipAlignment != 0) direction = Vector3.down;
        bool fired=false;
        foreach (var module in modules.Where(x=>x.data.type==ModuleType.Canon))
        {
            if (module.FireCanon(direction,this.GameObject())) fired = true;
        }
        return fired;
    }
    public bool FireMissle()
    {
        foreach (var module in modules.Where(x=>x.data.type==ModuleType.Missile))
        {
            if (module.FireMissile(Vector2.one, this.GameObject())) return true;
        }
        return false;
    }
    public Vector3 GetGridCenterLocal()
    {
        if (modules.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var m in modules)
        {
            sum += m.transform.localPosition; // локальные координаты относительно ModuleParent
        }
        return sum / modules.Count;
    }
}