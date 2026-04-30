using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public int health = 1;
    public int damage = 0;
    public float lifetime = 15f;
    private float lifeTimer;
    public GameObject owner;
    public Faction ownerShipFaction;

    public abstract void Launch(Vector2 directionOrDummy, Vector2 targetPos,int projDamage, GameObject ownerShip = null);
    
    protected virtual void OnEnable()
    {
        lifeTimer = lifetime;
    }
    protected virtual void Update()
    {
        lifeTimer -= Time.deltaTime;

        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}