using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public int damage = 10;
    public GameObject owner;

    public abstract void Launch(Vector2 directionOrDummy, Vector2 targetPos, GameObject ownerShip = null);
}