using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum ModuleType { Canon, Missile, PointDefense, Speed, Shield, Empty }
public enum OutfitType { Canon, Missile, PointDefense, Empty }

[RequireComponent(typeof(PolygonCollider2D))]
public class ShipModule : MonoBehaviour
{
    public ShipModuleStats data;
    public Vector2Int gridPosition;
    public int currentRotation;            // 0, 90, 180, 270
    public int currentHP = 0;
    public float speedBonus = 0f;
    public float cooldown = 1f;
    private float lastShot;
    public PolygonCollider2D polyCollider;
    [SerializeField]private InertialBody inertialBody;
    [SerializeField]private ModuleBuilder builder;
    private void Awake()
    {
        inertialBody = GetComponent<InertialBody>();
        polyCollider = GetComponent<PolygonCollider2D>();
    }
    public void Initialize(ShipModuleStats newData,int rotation=0)
    {
        data = newData;
        currentRotation = rotation;
        currentHP = data.baseHealth;
        builder.Initialize(data,currentRotation);
        GenerateCollider();
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

    public void FireCanon(Vector3 direction)
    {
        if (Time.time - lastShot < cooldown) return;
        lastShot = Time.time;

        if (data.type == ModuleType.Canon)
        {
            Projectile bullet = Projectile.Spawn(transform.position, direction, 10f, 1);
        }
        // TODO: Missile, PD, etc.
    }
    public void OnAttachToShip(InertialBody newInertialBody)
    {
        if (inertialBody != null)
        {
            inertialBody.enabled = false;
            inertialBody.velocity = Vector2.zero;
            inertialBody = newInertialBody;
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