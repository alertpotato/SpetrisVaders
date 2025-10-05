using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public int damage = 0;
    public GameObject owner;

    public abstract void Launch(Vector2 directionOrDummy, Vector2 targetPos,int projDamage, GameObject ownerShip = null);
}