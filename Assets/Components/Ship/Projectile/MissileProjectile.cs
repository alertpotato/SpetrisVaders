using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MissileProjectile : Projectile
{
    [Header("Missile Settings")]
    public float initialSpeed = 10f;
    public float acceleration = 10f;
    public float accuracyRadius = 3f;
    public float arcHeight = 0.35f;
    public float forwardFactor = 0.6f;

    // state
    private Vector2 start;
    private Vector2 target;
    private Vector2 controlPoint;
    private Vector2 initialDir;
    private float distance;
    private float t;
    private float currentSpeed;
    private float elapsedTime;
    
    private bool passedTarget = false;
    private Vector2 velocity;

    private void Awake()
    {
        var adapter = GetComponent<DamageAdapter>();
        if (adapter != null)
        {
            adapter.owner = owner;
            adapter.TakeDamage.AddListener(OnTakeDamage);
        }
    }

    public override void Launch(Vector2 initialDirection, Vector2 targetPos, int projDamage, GameObject ownerShip = null)
    {
        health = 1;
        lifetime = 10f;
        damage = projDamage;
        owner = ownerShip;
        ownerShipFaction = owner !=null? owner.GetComponent<Ship>().faction : Faction.Neutral;
        initialDir = (initialDirection.sqrMagnitude > 0.0001f) ? initialDirection.normalized : Vector2.right;
        start = transform.position;
        GetComponent<DamageAdapter>().owner = owner;
        
        // Accuracy
        Vector2 randomOffset = Random.insideUnitCircle * accuracyRadius;
        target = targetPos + randomOffset;

        distance = Vector2.Distance(start, target);
        if (distance < 0.0001f) distance = 0.0001f;

        // forward смещает controlPoint вперёд по initialDir
        float forward = distance * (forwardFactor + Random.Range(0, 0.4f));
        // adjustment based on both ships position
        float verticalSign = (target.y >= start.y) ? -1f : 1f;
        float vertical = distance * arcHeight / 2 * verticalSign;

        controlPoint = start + initialDir * forward + Vector2.up * vertical;

        // init params
        t = 0f;
        elapsedTime = 0f;
        currentSpeed = initialSpeed;
        velocity = initialDir * initialSpeed;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        currentSpeed += acceleration * Time.deltaTime;

        if (!passedTarget)
        {
            // flying on curve
            float prevT = t;
            t += Time.deltaTime * currentSpeed / distance;
            t = Mathf.Clamp01(t);

            Vector2 prevPos = QuadraticBezier(start, controlPoint, target, prevT);
            Vector2 nextPos = QuadraticBezier(start, controlPoint, target, t);
            
            Vector2 dir = (nextPos - prevPos).normalized;
            velocity = dir * currentSpeed;

            transform.position = nextPos;
            if (dir.sqrMagnitude > 0.001f) transform.right = dir;

            if (t >= 1f) passedTarget = true;
        }
        else
        {
            Vector2 dir = velocity.normalized; 
            velocity = 1.5f * dir * currentSpeed;

            transform.position += (Vector3)(velocity * Time.deltaTime);
            if (dir.sqrMagnitude > 0.001f) transform.right = dir;
        }
    }

    private Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float tt)
    {
        Vector2 ab = Vector2.Lerp(a, b, tt);
        Vector2 bc = Vector2.Lerp(b, c, tt);
        return Vector2.Lerp(ab, bc, tt);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<DamageAdapter>()?.owner == owner) return;
        collision.gameObject.GetComponent<DamageAdapter>()?.TakeDamage.Invoke(damage);

        ProjectileManager.Instance.SpawnImpactEffect(transform.position, velocity);
        Destroy(gameObject);
    }

    private void OnTakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(this.GameObject());
        }
    }
}
