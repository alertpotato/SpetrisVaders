using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed;
    public int Damage;
    public Vector3 Direction;

    public static Projectile Spawn(Vector3 pos, Vector3 dir, float speed, int dmg)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.2f, 0.2f, 1);
        Projectile p = go.AddComponent<Projectile>();
        p.Direction = dir.normalized;
        p.Speed = speed;
        p.Damage = dmg;
        return p;
    }

    private void Update()
    {
        transform.position += Direction * Speed * Time.deltaTime;
    }
}