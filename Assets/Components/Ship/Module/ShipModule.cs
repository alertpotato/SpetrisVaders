using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.PlayerLoop;

public enum ModuleType { Canon, Missile, PointDefense, Speed, Shield, Cockpit, Hull, Empty }
public enum OutfitType { Canon, Missile, PointDefense, Shield, Empty, Cockpit }
public class ModuleCell
{
    public Transform firePoint;
    public ModuleCellScript script;
    public GameObject visualizer;
    public float lastShotTime;
}
[RequireComponent(typeof(PolygonCollider2D))]
public class ShipModule : MonoBehaviour
{
    [Header("Components")]
    public PolygonCollider2D polyCollider;
    [SerializeField]private InertialBody inertialBody;
    public ModuleBuilder builder;
    public GameObject owner;
    public ShipModuleStats data;
    
    [Header("Module variables")]
    public int currentRotation;            // 0, 90, 180, 270
    public int currentHP = 40;
    public float speedBonus = 0f;
    public Vector2Int gridPosition;
    public bool isFunctioning = true;
    
    [Header("Outfit stats")]
    public float cooldown = 1f;
    public int damage=0;
    private float lastShot;
    private float maxRange;
    private float accuracy;
    private Vector3 projectileAdjustment= Vector3.zero;
    public List<ModuleCell> outfitCells = new List<ModuleCell>();

    
   
    private void Awake()
    {
        inertialBody = GetComponent<InertialBody>();
        polyCollider = GetComponent<PolygonCollider2D>();
        transform.AddComponent<DamageVizualizer>();
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
        maxRange = data.maxRange;
        accuracy = data.accuracy;
        builder.Initialize(data,currentRotation);
        GenerateCollider();
        
        var adapter = GetComponent<DamageAdapter>();
        if (adapter != null)
        {
            adapter.owner = owner;
            adapter.TakeDamage.AddListener(OnTakeDamage);
        }
        if (data.type == ModuleType.PointDefense) transform.AddComponent<PointDefenseSystem>();
        var pd = transform.GetComponent<PointDefenseSystem>(); if (pd!=null) pd.enabled = false;
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
    public bool FirePD(Vector3 direction,GameObject parent)
    {
        if (Time.time - lastShot < cooldown) return false;
        if (data.type != ModuleType.PointDefense) return false;
        lastShot = Time.time;
        var spread = GetSpreadRadius(accuracy,maxRange,maxRange);
        foreach (var cell in outfitCells)
        {
            var to = cell.firePoint.position + direction.normalized * maxRange;
                     ProjectileManager.Instance.SpawnPointDefenseShot(cell.firePoint.position,to,damage,maxRange,spread,owner);
        }
        return true;
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
    public bool FireMissile(Vector3 target,GameObject parent)
    {
        if (Time.time - lastShot < cooldown) return false;
        if (data.type != ModuleType.Missile) return false;
        
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
                ProjectileManager.Instance.SpawnMissile(builder.cells[i].transform.position, target,startDirection,damage, parent);
        }
        return true;
    }
    public bool FireMissile(List<Ship> targets,GameObject parent)
    {
        if (Time.time - lastShot < cooldown) return false;
        if (data.type != ModuleType.Missile) return false;
        if (targets.Count == 0) return false;
        //find closest target
        Vector2 missileTarget=new Vector2(-999,-999);
        float distToTarget=999;
        foreach (var target in targets)
        {
            var newDist = Vector3.Distance(target.transform.position, this.transform.position);
            if (newDist <= maxRange && distToTarget > newDist) {distToTarget = newDist; missileTarget=target.transform.position;}
        }
        if (missileTarget==new Vector2(-999,-999)) return false;
        
        lastShot = Time.time;
        
        Vector3 gridCenter = parent.GetComponent<Ship>().GetGridCenterLocal();
        Vector3 startDirection = Vector3.zero;
        if (transform.localPosition.x > gridCenter.x)
            startDirection = Vector3.right;
        else
            startDirection = Vector3.left;
        Debug.DrawLine(this.transform.position,missileTarget,Color.red,3f);
        for (int i = 0; i < data.shape.Length; i++)
        {
            if (data.shape[i].type == OutfitType.Missile)
                ProjectileManager.Instance.SpawnMissile(builder.cells[i].transform.position, missileTarget,startDirection,damage, parent);
        }
        return true;
    }

    public void OnAttachToShip(GameObject ship, InertialBody newInertialBody, int alignment)
    {
        owner = ship;
        var adapter = GetComponent<DamageAdapter>();
        if (adapter != null)
        {
            adapter.owner = ship;
        }
        //Inertial body logic
        if (inertialBody != null)
        {
            inertialBody.enabled = false;
            inertialBody.velocity = Vector2.zero;
            inertialBody = newInertialBody;
        }
        //aligment player vs enemy
        foreach (var cell in builder.cells)
        {
            cell.transform.rotation = Quaternion.Euler(0,0,alignment);
            projectileAdjustment = new Vector3(0f, alignment == 0 ? 0.5f : -0.5f, 0f);
        }
        //regenerate collider just in case
        GenerateCollider();
        //pd activator
        var pd = transform.GetComponent<PointDefenseSystem>();
        if (pd != null) {pd.enabled = true; pd.Initialize(); }
        //init cells, if player - create some vizual staff for each outfit
        InitializeCells(ship.GetComponent<Ship>().faction);
        //inherit ship layer
        gameObject.layer = ship.layer;
    }
    public void OnDetachFromShip()
    {
        owner = gameObject;
        transform.SetParent(null);
        
        inertialBody = GetComponent<InertialBody>();
        if(inertialBody!=null) inertialBody.enabled = true;

        var adapter = GetComponent<DamageAdapter>();
        if (adapter != null)
            adapter.owner = owner;

        var pd = GetComponent<PointDefenseSystem>();
        if (pd != null)
            pd.enabled = false;
        
        DropCells();
    }

    public void OnTakeDamage(int damage)
    {
        if (data.type== ModuleType.Shield && lastShot < cooldown) {lastShot = Time.time;return;}
        currentHP = Mathf.Clamp(currentHP - damage,0,data.baseHealth);
        if (data.type==ModuleType.Cockpit && currentHP<21 && owner.GetComponent<Ship>().HUDConsole != null) 
            owner.GetComponent<Ship>().HUDConsole.EnqueueMessage("> CRITICAL HIT — CORE TEMPERATURE RISING",ConsoleMessageType.WARNING);
        if (currentHP <= 0)
        {
            isFunctioning = false;
            owner?.GetComponent<Ship>()?.OnModuleBroken(this);
            foreach (var cell in builder.cells)
            {
                cell.GetComponent<ModuleCellScript>().mainSprite.color = new Color(0.5f,0.5f,0.5f,1f);
            }
            polyCollider.enabled = false;
            var pd = transform.GetComponent<PointDefenseSystem>(); if (pd!=null) pd.enabled = false;
            //Destroy(this.GameObject());
            return;
        }
    }

    public void UpdateRotation(int newRotation)
    {
        currentRotation = newRotation;
        builder.UpdateModule(newRotation);
    }
    private float GetSpreadRadius(float maxSpread, float distanceToTarget, float maxDistance)
    {
        float radius = 0;
        radius = maxSpread * Mathf.Clamp(distanceToTarget / maxDistance, 0, 1);
        return radius;
    }
    
    //--------------------CELLS CONTROL
    private void InitializeCells(Faction faction)
    {
        for (int i = 0; i < data.shape.Length; i++)
        {
            if (data.shape[i].type == OutfitType.Empty) continue;

            var cell = builder.cells[i];
            var script = cell.GetComponent<ModuleCellScript>();

            GameObject visual = null;
            if (faction == Faction.Player)
            {
                visual = new GameObject(data.shape[i].type + "_Line_" + i);
                visual.transform.SetParent(cell.transform);
                LineRenderer lr = visual.AddComponent<LineRenderer>();
                lr.material = GameGraphics.Instance.simpleHDRColor;
                lr.startColor = new Color(1f,1f,1f,0.7f);
                lr.endColor = new Color(1f,1f,1f,0f);
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.positionCount = 2;
                lr.sortingOrder = 60;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.enabled = false;
            }

            outfitCells.Add(new ModuleCell
            {
                firePoint = cell.transform,
                script = script,
                visualizer = visual
            });
        }
    }

    public void LookAt(Vector2 target)
    {
        foreach (var cell in outfitCells)
        {
            Vector2 pos = (Vector2)cell.firePoint.position;
            Vector2 dir = (target - pos);
            Vector2 dirN = dir.normalized;
            float angle = Mathf.Atan2(dirN.y, dirN.x) * Mathf.Rad2Deg;

            if (cell.script?.outfitSprite != null)
                cell.script.outfitSprite.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            if (cell.visualizer!=null)
            {
                var line = cell.visualizer.GetComponent<LineRenderer>();
                line.enabled = true;
                line.SetPosition(0, pos+dirN/2);
                line.SetPosition(1, pos + dirN * Mathf.Min(dir.magnitude,maxRange/2));
            }
        }
    }
    public void DisableCells()
    {
        foreach (var cell in outfitCells)
        {
            if (cell.visualizer != null)
            {
                var line = cell.visualizer.GetComponent<LineRenderer>();
                line.enabled = false;
            }
        }
    }

    private void DropCells()
    {
        foreach (var cell in outfitCells)
        {
            Destroy(cell.visualizer);
        }
        outfitCells.Clear();
    }
}