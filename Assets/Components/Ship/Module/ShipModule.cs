using UnityEngine;
using UnityEngine.PlayerLoop;

public enum ModuleType { Canon, Missile, PointDefense, Speed, Shield, Empty }
public enum OutfitType { Canon, Missile, PointDefense, Empty }

public class ShipModule : MonoBehaviour
{
    public ShipModuleStats data;
    public Vector2Int gridPosition;
    public int rotation;            // 0, 90, 180, 270
    public int currentHP = 0;
    public float speedBonus = 0f;
    public float cooldown = 1f;
    private float lastShot;
    [SerializeField]private PolygonCollider2D polyCollider;
    [SerializeField]private ModuleBuilder builder;

    public void Initialize(ShipModuleStats newData)
    {
        data = newData;
        currentHP = data.baseHealth;
        builder.Initialize(data);
        GenerateCollider();
    }

    private void GenerateCollider()
    {
        polyCollider.pathCount = data.shape.Length;

        for (int i = 0; i < data.shape.Length; i++)
        {
            Vector2 cell = data.shape[i].localPosition;
            Vector2[] square = new Vector2[4];

            // квадрат размером 1x1
            square[0] = cell + new Vector2(-0.5f, -0.5f);
            square[1] = cell + new Vector2(-0.5f,  0.5f);
            square[2] = cell + new Vector2( 0.5f,  0.5f);
            square[3] = cell + new Vector2( 0.5f, -0.5f);

            polyCollider.SetPath(i, square);
        }
    }
    
    public Vector2Int[] GetRotatedCells()
    {
        Vector2Int[] result = new Vector2Int[data.shape.Length];
        for (int i = 0; i < data.shape.Length; i++)
        {
            result[i] = RotateCell(data.shape[i].localPosition, rotation);
        }
        return result;
    }
    
    private Vector2Int RotateCell(Vector2Int cell, int rotation)
    {
        switch (rotation % 360)
        {
            case 90:  return new Vector2Int(-cell.y,  cell.x);
            case 180: return new Vector2Int(-cell.x, -cell.y);
            case 270: return new Vector2Int( cell.y, -cell.x);
            default:  return cell;
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

    public void ModuleInitialization(ModuleType type)
    {

    }
}