using UnityEngine;
using System.Collections.Generic;

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
        grid.Attach(candidateModule.module.GetComponent<ShipModule>(), anchor,adjustment);
        candidateModule.module.transform.SetParent(ModuleParent.transform);
        candidateModule.module.transform.localPosition = new Vector3(anchor.x+adjustment.x, anchor.y+adjustment.y, 0);
        candidateModule.module.GetComponent<ShipModule>().OnAttachToShip(inertialBody);
        modules.Add(candidateModule.module.GetComponent<ShipModule>());
        UpdateStats();
    }
}