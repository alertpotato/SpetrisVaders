using UnityEngine;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;

    public GameObject missilePrefab;
    public GameObject shellPrefab;
    [SerializeField]private ParticleSystem impact;
    [SerializeField]private GameObject shellsParent;
    [SerializeField]private GameObject missileParent;

    private List<Projectile> activeProjectiles = new List<Projectile>();

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

    public void SpawnImpactEffect(Vector3 position,Vector2 direction)
    {
        float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ParticleSystem impactM = Instantiate(impact,position,Quaternion.Euler(0f, 0f, 360 - (angleZ + 10)));
        impactM.transform.SetParent(gameObject.transform);
    }
}