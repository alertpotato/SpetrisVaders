using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum ModuleType { Canon, Missile, PointDefense, Speed, Shield, Empty }
public enum OutfitType { Canon, Missile, PointDefense, Empty }

[RequireComponent(typeof(PolygonCollider2D))]
public class ShipModule : MonoBehaviour
{
    public ShipModuleStats data;
    public int currentRotation;            // 0, 90, 180, 270
    public int currentHP = 40;
    public float speedBonus = 0f;
    public float cooldown = 1f;
    public int damage=0;
    private float lastShot;
    public PolygonCollider2D polyCollider;
    [SerializeField]private InertialBody inertialBody;
    [SerializeField]private ModuleBuilder builder;
    [SerializeField]private GameObject owner;
    private Vector3 projectileAdjustment= Vector3.zero;
    public Vector2Int gridPosition;
    private void Awake()
    {
        inertialBody = GetComponent<InertialBody>();
        polyCollider = GetComponent<PolygonCollider2D>();
    }
    public void Initialize(ShipModuleStats newData,int rotation=0)
    {
        owner = gameObject;
        data = newData;
        currentRotation = rotation;
        currentHP = data.baseHealth;
        cooldown = data.cooldown;
        speedBonus = data.speedModifier;
        damage = data.damage;
        builder.Initialize(data,currentRotation);
        GenerateCollider();
        
        var adapter = GetComponent<DamageAdapter>();
        if (adapter != null)
        {
            adapter.owner = owner;
            adapter.TakeDamage.AddListener(OnTakeDamage);
        }
    }

    private void GenerateCollider()
    {
        polyCollider.pathCount = data.shape.Length;

        for (int i = 0; i < data.shape.Length; i++)
        {
            Vector2 cell = builder.RotateCell(data.shape[i].localPosition,currentRotation);
            Vector2[] square = new Vector2[4];

            // квадрат размером 1x1
            square[0] = cell + new Vector2(-0.5f, -0.5f);
            square[1] = cell + new Vector2(-0.5f,  0.5f);
            square[2] = cell + new Vector2( 0.5f,  0.5f);
            square[3] = cell + new Vector2( 0.5f, -0.5f);

            polyCollider.SetPath(i, square);
        }
    }

    public bool FireCanon(Vector3 direction,GameObject parent)
    {
        if (Time.time - lastShot < cooldown) return false;
        if (data.type != ModuleType.Canon) return false;
        lastShot = Time.time;
        
        for (int i = 0; i < data.shape.Length; i++)
        {
            if (data.shape[i].type == OutfitType.Canon)
                ProjectileManager.Instance.SpawnShell(builder.cells[i].transform.position+projectileAdjustment, direction,damage,parent);
        }
        return true;
    }
    public bool FireMissile(List<Ship> targets,GameObject parent)
    {
        if (Time.time - lastShot < cooldown) return false;
        if (data.type != ModuleType.Missile) return false;
        if (targets.Count == 0) return false;
        lastShot = Time.time;
        
        Vector3 gridCenter = parent.GetComponent<Ship>().GetGridCenterLocal();
        Vector3 startDirection = Vector3.zero;
        if (transform.localPosition.x > gridCenter.x)
            startDirection = Vector3.right;
        else
            startDirection = Vector3.left;
        
        for (int i = 0; i < data.shape.Length; i++)
        {
            if (data.shape[i].type == OutfitType.Missile)
                ProjectileManager.Instance.SpawnMissile(builder.cells[i].transform.position, targets.First().transform.position,startDirection,damage, parent);
        }
        return true;
    }
    public void OnAttachToShip(GameObject ship,InertialBody newInertialBody,int alignment)
    {
        owner = ship;
        var adapter = GetComponent<DamageAdapter>();
        if (adapter != null)
        {
            adapter.owner = ship;
        }
        
        if (inertialBody != null)
        {
            inertialBody.enabled = false;
            inertialBody.velocity = Vector2.zero;
            inertialBody = newInertialBody;
        }
        foreach (var cell in builder.cells)
        {
            cell.transform.Rotate(0,0,alignment);
            projectileAdjustment=new Vector3(0f,alignment==0?0.5f:-0.5f,0f);
        }
        GenerateCollider();
    }
    public void OnDetachFromShip()
    {
        transform.SetParent(null);
        if (inertialBody != null)
        {
            inertialBody = GetComponent<InertialBody>();
            inertialBody.enabled = true;
        }
    }
    public void OnTakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"Damage {damage} HP {currentHP}");
        if (currentHP <= 0)
        {
            owner.GetComponent<Ship>()?.OnModuleDestroyed(this);
            Destroy(this.GameObject());
        }
    }

    public void UpdateRotation(int newRotation)
    {
        currentRotation = newRotation;
        builder.UpdateModule(newRotation);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Началось пересечение с " + other.name);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Закончилось пересечение с " + other.name);
    }

}