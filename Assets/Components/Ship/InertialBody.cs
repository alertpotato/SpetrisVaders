using UnityEngine;
using System.Collections.Generic;

public class InertialBody : MonoBehaviour
{
    public float mass = 1f;
    public float drag = 0.95f;
    public float maxSpeed = 10f;
    public Vector2 velocity;
    public List<PolygonCollider2D> colliders = new List<PolygonCollider2D>();
    private void OnEnable()
    {
        if (colliders.Count == 0) colliders.Add(GetComponent<PolygonCollider2D>());
        InertialCollisionManager.Instance?.Register(this,colliders);
    }

    private void OnDisable()
    {
        Debug.Log("Disabling InertialBody" + this.transform.name);
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
        velocity += (force / mass) * deltaTime;
    }

    public void Tick(float deltaTime,bool isForceApplied = false)
    {
        if (velocity.magnitude > maxSpeed)
            velocity = velocity.normalized * maxSpeed;

        transform.position += (Vector3)(velocity * deltaTime);
        if (!isForceApplied) velocity *= drag;
        if (velocity.magnitude<0.01f) velocity = Vector2.zero;
    }
}
