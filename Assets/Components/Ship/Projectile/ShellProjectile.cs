using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;

public class ShellProjectile : Projectile
{
    public float speed = 20f;
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
    public override void Launch(Vector2 direction, Vector2 targetPos,int projDamage, GameObject ownerShip = null)
    {
        damage = projDamage;
        owner = ownerShip;
        ownerShipFaction = owner !=null? owner.GetComponent<Ship>().faction : Faction.Neutral;
        velocity = direction.normalized * speed;
        GetComponent<DamageAdapter>().owner = owner;
        transform.right = direction;
        //TODO proper destroy?
        Destroy(this.gameObject, lifetime);
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
    private void OnTakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(this.GameObject());
        }
    }
}