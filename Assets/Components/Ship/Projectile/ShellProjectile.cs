using UnityEngine;
using UnityEngine.TestTools;

public class ShellProjectile : Projectile
{
    public float speed = 10f;
    private Vector2 velocity;

    public override void Launch(Vector2 direction, Vector2 targetPos,int projDamage, GameObject ownerShip = null)
    {
        damage = projDamage;
        owner = ownerShip;
        velocity = direction.normalized * speed;
    }

    void Update()
    {
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<DamageAdapter>()?.owner == owner) return;
        
        collision.gameObject.GetComponent<DamageAdapter>()?.TakeDamage.Invoke(damage);
        ProjectileManager.Instance.SpawnImpactEffect(transform.position,velocity);
        Destroy(gameObject);
    }
}