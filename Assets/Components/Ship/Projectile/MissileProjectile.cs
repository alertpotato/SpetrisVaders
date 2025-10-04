using UnityEngine;

public class MissileProjectile : Projectile
{
    [Header("Missile Settings")]
    public float initialSpeed = 2f;   // стартовая скорость
    public float maxSpeed = 8f;       // максимальная скорость
    public float acceleration = 5f;   // ускорение
    public float arcHeight = 1.5f;    // высота дуги
    public float lifetime = 5f;       // максимальное время жизни

    private Vector2 start;
    private Vector2 target;
    private Vector2 controlPoint;
    private float t;
    private float currentSpeed;
    private float elapsedTime;

    public override void Launch(Vector2 dir, Vector2 targetPos, GameObject ownerShip = null)
    {
        damage = 10;
        owner = ownerShip;
        start = transform.position;
        target = targetPos;

        // lateralOffset: заворот в сторону стартового положения относительно центра
        Vector2 lateralOffset = Vector2.right * Mathf.Sign(start.x - owner.transform.position.x) * 1f;
        controlPoint = (start + target) / 2 + Vector2.up * arcHeight + lateralOffset;

        t = 0f;
        currentSpeed = initialSpeed;
        elapsedTime = 0f;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // ускорение до максимальной скорости
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

        // параметр t вдоль кривой
        t += Time.deltaTime * currentSpeed / Vector2.Distance(start, target);
        t = Mathf.Clamp01(t);

        Vector2 nextPos = QuadraticBezier(start, controlPoint, target, t);
        transform.position = nextPos;

        // разворот спрайта по направлению движения
        Vector2 dir = (target - nextPos).normalized;
        if (dir.sqrMagnitude > 0.001f)
            transform.right = dir;

        // достигли цели
        if (t >= 1f)
            Destroy(gameObject);
    }

    private Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 ab = Vector2.Lerp(a, b, t);
        Vector2 bc = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(ab, bc, t);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner) return;
        ProjectileManager.Instance.SpawnImpactEffect(transform.position,(Vector2)transform.position - target);
    }
}
