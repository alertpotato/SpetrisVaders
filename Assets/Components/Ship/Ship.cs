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
        maxSpeed = 99f;
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
}