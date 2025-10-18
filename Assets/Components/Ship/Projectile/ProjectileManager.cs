using UnityEngine;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;

    public GameObject missilePrefab;
    public GameObject shellPrefab;
    
    [SerializeField]private GameObject shellsParent;
    [SerializeField]private GameObject missileParent;
    
    [SerializeField]private ParticleSystem impactDirected;
    [SerializeField]private ParticleSystem bullets;
    [SerializeField]private ParticleSystem impactMetal;
    
    public List<Projectile> activeProjectiles = new List<Projectile>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        activeProjectiles.RemoveAll(p => p == null);
    }

    public void SpawnMissile(Vector3 spawnPos, Vector2 targetPos,Vector3 startDirection,int damage, GameObject owner)
    {
        GameObject missileObj = Instantiate(missilePrefab, spawnPos, Quaternion.identity);
        MissileProjectile missile = missileObj.GetComponent<MissileProjectile>();
        missile.Launch(startDirection, targetPos,damage, owner);
        missile.transform.SetParent(missileParent.transform);
        activeProjectiles.Add(missile);
    }

    public void SpawnShell(Vector3 spawnPos, Vector2 direction,int damage, GameObject owner)
    {
        GameObject shellObj = Instantiate(shellPrefab, spawnPos, Quaternion.identity);
        ShellProjectile shell = shellObj.GetComponent<ShellProjectile>();
        shell.Launch(direction, Vector2.zero,damage, owner);
        shell.transform.SetParent(shellsParent.transform);
        activeProjectiles.Add(shell);
    }

    public void SpawnPointDefenseShot(Vector3 from, Vector3 to, int damage, float range,float spread, GameObject owner)
    {
        SpawnBulletEffect(from, to,  spread, range,owner.transform);
        Vector2 dir = (to - from).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(from, dir, range);
        foreach (var hit in hits)
        {
            var adapter = hit.collider.GetComponent<DamageAdapter>();
            if (adapter != null && adapter.owner != owner)
            {
                adapter.TakeDamage?.Invoke(damage);
                Debug.DrawLine(from, hit.point, Color.blue, 5f);
                return;
            }
        }
        //Debug.DrawLine(from, from + (Vector3)dir * range, Color.red, 5f);
    }
    public void SpawnBulletEffect(Vector3 from, Vector3 to,float radiusSpread, float maxRange,Transform origin)
    {
        //direction and arc based on bullet accuracy
        var direction = to - from;
        float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float arc = Mathf.Rad2Deg * 2f * Mathf.Atan(radiusSpread / maxRange);
        ParticleSystem bullet = Instantiate(bullets,from,Quaternion.Euler(0f, 0f, angleZ-arc/2));
        var shape = bullet.shape;
        shape.arc = arc;
        //lifetime based on range and speed
        var main = bullet.main;
        main.startLifetime = maxRange / main.startSpeed.constant;
        //ignoring its own collision
        var collision = bullet.collision; 
        collision.collidesWith = LayerMask.GetMask("PlayerShip", "EnemyShip", "Environment", "Projectile");
        int ownerLayer = origin.gameObject.layer;
        collision.collidesWith &= ~(1 << ownerLayer);
        
        bullet.transform.SetParent(origin);
    }

    public void SpawnImpactEffect(Vector3 position,Vector2 direction)
    {
        float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ParticleSystem impactM = Instantiate(impactMetal,position,Quaternion.identity);
    }
}