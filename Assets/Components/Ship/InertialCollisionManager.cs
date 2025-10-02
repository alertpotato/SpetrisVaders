using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InertialCollisionManager : MonoBehaviour
{
    public static InertialCollisionManager Instance;

    public Dictionary<InertialBody,List<PolygonCollider2D>> bodies = new();
    
    private void Awake()
    {
        Instance = this;
    }

    public void Register(InertialBody body,List<PolygonCollider2D> colliders)
    {
        bodies[body] = colliders;
    }

    public void Unregister(InertialBody body)
    {
        bodies.Remove(body);
    }

    private void LateUpdate()
    {
        ResolveCollisions();
    }

    void ResolveCollisions()
    {
        var bodiesListA = bodies.Keys.ToList();
        var bodiesListB = bodies.Keys.ToList();
        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                InertialBody a = bodiesListA[i];
                InertialBody b = bodiesListB[j];

                if (a == null || b == null) continue;
                if (a==b) continue;
                foreach (var collA in bodies[a])
                {
                    foreach (var collB in bodies[b])
                    {
                        if (collA == null || collB == null) continue;
                        if (collA.IsTouching(collB))
                        {
                            HandleCollision(a, b);
                            //Debug.Log($"{a.transform.name} : {b.transform.name} : {collA} : {collB}");
                        }
                    }
                }
            }
        }
    }

    void HandleCollision(InertialBody a, InertialBody b)
    {
        Vector2 normal = (a.transform.position - b.transform.position).normalized;

        float relVel = Vector2.Dot(a.velocity - b.velocity, normal);

        if (relVel > 0) relVel=-0.01f;

        float e = 1f;

        float j = -(e) * relVel;
        j /= (1 / a.mass + 1 / b.mass);

        Vector2 impulse = j * normal;

        a.velocity += impulse / a.mass;
        b.velocity -= impulse / b.mass;
    }
}