using System;
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
    public List<ShipModule> hullModules = new List<ShipModule>();
    [SerializeField]private GameObject ModuleParent;
    public ShipModule cockpit;
    public TypewriterMessageQueue HUDConsole;
    
    [Header("Ship stats")]
    [Header("Ship stats")]
    public int shipAlignment = 180;
    public float thrust = 10;
    public float maxSpeed = 10;
    public Faction faction;
    
    [Header("Weapon settings")]
    public float canonFireCooldown = 0.1f;
    public float missileFireCooldown = 0.2f;
    public float lastShot;
    
    [Header("Variables")]
    public Vector2 dimensionsMin = new Vector2(0, 0);
    public Vector2 dimensionsMax = new Vector2(0, 0);
    public List<ShipModule> controlledModules = new List<ShipModule>();
    public event Action<Ship> OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }
    public void InitializeShip(Faction shipFaction)
    { 
        faction = shipFaction;
    }
    
    private void UpdateStats()
    {
        maxSpeed = 10f;
        thrust = 20f;
        var colliders = new List<PolygonCollider2D>();
        foreach (var module in modules)
        {
            maxSpeed += module.speedBonus;
            thrust += 5;
            colliders.Add(module.polyCollider);
        }
        
        inertialBody.UpdateBody(modules.Count,maxSpeed,colliders);
        UpdateShipDimensions();
    }

    public void AttachModule(GameObject moduleG, Vector2Int anchor, Vector2Int adjustment)
    {
        var module = moduleG.GetComponent<ShipModule>();
    
        grid.Attach(module, anchor, adjustment);
        module.transform.SetParent(ModuleParent.transform);
        module.transform.localPosition = new Vector3(anchor.x + adjustment.x, anchor.y + adjustment.y, 0);
        module.OnAttachToShip(this.gameObject, inertialBody, shipAlignment);
        modules.Add(module);
        
        //TODO make it more safe?
        if (cockpit == null)
            cockpit = module;
        
        //Fill certain spaces with hull-modules
        foreach (var hullPos in grid.GetMirrorHullPositions(module))
        {
            var hull = ModuleFactory.Instance.GetHullModule(transform);
            var hullS = hull.GetComponent<ShipModule>();
            if(!grid.AttachHull(hullS, hullPos)) continue;
            hullS.transform.SetParent(ModuleParent.transform);
            hullS.transform.localPosition = new Vector3(hullPos.x, hullPos.y, 0);
            hullS.OnAttachToShip(this.gameObject, inertialBody, shipAlignment);
            hullModules.Add(hullS);
        }
        //delete hulls that are overlapped by new module
        foreach (var h in grid.GetOverlapingHulls(module))
        {
            grid.RemoveHull(h);
            hullModules.Remove(h);
            Destroy(h.gameObject);
        }
        UpdateStats();
        if (HUDConsole!=null) HUDConsole.EnqueueMessage("> UPGRADE SUCCESSFUL. INSTALLED "+module.name.ToString().ToUpper());
    }

    public void OnModuleDestroyed(ShipModule module)
    {
        if (HUDConsole!=null && module.data.type!=ModuleType.Cockpit) HUDConsole.EnqueueMessage("> DAMAGE REPORT: LOST "+module.name.ToString().ToUpper(),ConsoleMessageType.WARNING);
        grid.RemoveModule(module);
        modules.Remove(module);
        var disconnectedModules = grid.GetDisconnectedModules(cockpit);
        Vector3 shipCenter = grid.GridToWorld(GetGridCenterLocal());
        
        foreach (var m in disconnectedModules)
        {
            modules.Remove(m);
            controlledModules.Remove(m);
            grid.RemoveModule(m);
            m.OnDetachFromShip(shipCenter);
        }
        
        if (module == cockpit)
        {
            if (HUDConsole!=null) HUDConsole.EnqueueMessage("> CRITICAL DA,MAA#AD@ /.|--....",ConsoleMessageType.WARNING);
            Destroy(gameObject);
            return;
        }
        UpdateStats();
    }

    public void UpdateModulesControl(Vector2 lookAt)
    {
        foreach (var module in controlledModules)
        {
            if (module==null) continue;
            module.LookAt(lookAt);
        }
    }
    public void ControlModulesByType(ModuleType type)
    {
        foreach (var module in controlledModules)
        {
            if (module==null) continue;
            module.DisableCells();
        }
        controlledModules.Clear();
        if (type == ModuleType.Empty) return;
        foreach (var module in modules.Where(x=>x.data.type==type))
        {
            controlledModules.Add(module);
        }
    }

    public bool FireAt(Vector3 position)
    {
        if (Time.time - lastShot < canonFireCooldown) return false;
        lastShot = Time.time;
        var direction = position - transform.position;
        foreach (var module in modules.Where(x=>x.data.type==ModuleType.Canon))
        {
            if (module.FireCanon(direction,this.GameObject())) return true;
        }
        return false;
    }
    public bool FireAtPD(Vector3 position)
    {
        var direction = position - transform.position;
        foreach (var module in modules.Where(x=>x.data.type==ModuleType.PointDefense))
        {
            if (module.FirePD(direction,this.GameObject())) return true;
        }
        return false;
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
    public bool FireMissle(Vector3 target)
    {
        if (Time.time - lastShot < missileFireCooldown) return false;
        lastShot = Time.time;
        
        foreach (var module in modules.Where(x=>x.data.type==ModuleType.Missile))
        {
            if (module.FireMissile( target,this.GameObject())) return true;
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
    
    public float DistanceToObject(Vector2 worldPoint)
    {
        float minDist = float.MaxValue;
        
        foreach (var module in modules)
        {
            if (module == null || module.polyCollider == null) continue;

            var poly = module.polyCollider;
            var points = poly.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 worldA = poly.transform.TransformPoint(points[i]);
                Vector2 worldB = poly.transform.TransformPoint(points[(i + 1) % points.Length]);

                float dist = DistancePointToSegment(worldPoint, worldA, worldB);
                if (dist < minDist) minDist = dist;
            }
        }

        return minDist;
    }

    public float DistanceToShip(Ship other)
    {
        float minDist = float.MaxValue;

        foreach (var module in modules)
        {
            if (module == null || module.polyCollider == null) continue;
            var polyA = module.polyCollider;

            foreach (var otherModule in other.modules)
            {
                if (otherModule == null || otherModule.polyCollider == null) continue;
                var polyB = otherModule.polyCollider;

                // Сравниваем все стороны двух полигонов
                var pointsA = polyA.points.Select(p => polyA.transform.TransformPoint(p)).ToArray();
                var pointsB = polyB.points.Select(p => polyB.transform.TransformPoint(p)).ToArray();

                for (int i = 0; i < pointsA.Length; i++)
                {
                    Vector2 a1 = pointsA[i];
                    Vector2 a2 = pointsA[(i + 1) % pointsA.Length];

                    for (int j = 0; j < pointsB.Length; j++)
                    {
                        Vector2 b1 = pointsB[j];
                        Vector2 b2 = pointsB[(j + 1) % pointsB.Length];

                        float dist = DistanceSegmentToSegment(a1, a2, b1, b2);
                        if (dist < minDist) minDist = dist;
                    }
                }
            }
        }

        return minDist;
    }

    /// <summary>
    /// Минимальное расстояние от точки до отрезка.
    /// </summary>
    private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector2 projection = a + t * ab;
        return Vector2.Distance(p, projection);
    }

    /// <summary>
    /// Минимальное расстояние между двумя отрезками.
    /// </summary>
    private float DistanceSegmentToSegment(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        // Переборный способ — быстрый и точный для 2D
        float[] dists = new float[]
        {
            DistancePointToSegment(a1, b1, b2),
            DistancePointToSegment(a2, b1, b2),
            DistancePointToSegment(b1, a1, a2),
            DistancePointToSegment(b2, a1, a2)
        };

        return dists.Min();
    }

    private void UpdateShipDimensions()
    {
        int MaxX = 0;
        int MaxY = 0;
        int MinX = 0;
        int MinY = 0;
        foreach (var cell in grid.grid.Keys.ToList())
        {
            if(cell.x > MaxX) MaxX = cell.x;
            if (cell.x < MinX) MinX = cell.x;
            if (cell.y > MaxY) MaxY = cell.y;
            if (cell.y < MinY) MinY = cell.y;
        }
        dimensionsMin = new Vector2(MinX, MinY);
        dimensionsMax = new Vector2(MaxX, MaxY);
    }
}