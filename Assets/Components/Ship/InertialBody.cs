using UnityEngine;
using System.Collections.Generic;

public class InertialBody : MonoBehaviour
{
    [Header("BodyProperties")]
    public float mass = 1f;
    public float drag = 0.95f;
    public float maxSpeed = 10f;
    public Vector2 velocity;
    [Header("CurrentProperties")]
    public float acceleration;
    public float speed;
    private List<PolygonCollider2D> colliders = new List<PolygonCollider2D>();
    private void OnEnable()
    {
        if (colliders.Count == 0) colliders.Add(GetComponent<PolygonCollider2D>());
        InertialCollisionManager.Instance?.Register(this,colliders);
    }

    private void OnDisable()
    {
        InertialCollisionManager.Instance?.Unregister(this);
    }

    public void UpdateBody(float newMass, float newMaxSpeed, List<PolygonCollider2D> newColliders)
    {
        mass = newMass;
        maxSpeed = newMaxSpeed;
        colliders = newColliders;
        
        InertialCollisionManager.Instance?.Register(this,newColliders);
    }

    public void ApplyForce(Vector2 force, float deltaTime)
    {
        if (force == Vector2.zero)
            return;
        
        Vector2 velocityDir = velocity.sqrMagnitude > 0.001f ? velocity.normalized : Vector2.zero;
        Vector2 forceDir = force.normalized;
        
        float alignment = Vector2.Dot(velocityDir, forceDir); // 1 = same, -1 = different
        //multiplier to force
        float resistanceMultiplier = Mathf.Lerp(2.5f, 1f, Mathf.Max(0, alignment)); 
        Vector2 adjustedForce = force * resistanceMultiplier;
        velocity += (adjustedForce / mass) * deltaTime;
        acceleration = (force / mass).magnitude;
        speed = velocity.magnitude;
    }

    public void Tick(float deltaTime,float maxSpeedCorrection=99,bool isForceApplied = false)
    {
        var maxSpeedActual = Mathf.Min(maxSpeed, maxSpeedCorrection);
        if (velocity.magnitude > maxSpeedActual)
            velocity = velocity.normalized * maxSpeedActual;

        transform.position += (Vector3)(velocity * deltaTime);
        if (!isForceApplied) velocity *= drag;
        if (velocity.magnitude<0.01f) velocity = Vector2.zero;
    }
}
