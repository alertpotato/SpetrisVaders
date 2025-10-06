using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public int health = 1;
    public int damage = 0;
    public float lifetime = 30f;
    public GameObject owner;
    public Faction ownerShipFaction;

    public abstract void Launch(Vector2 directionOrDummy, Vector2 targetPos,int projDamage, GameObject ownerShip = null);
}