using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ShipModule))]
public class PointDefenseSystem : MonoBehaviour
{
    [Header("Settings")]
    public float cooldown = 0.5f;
    public float maxRange = 10f;
    public float scanRange = 20f;
    public int burstCount = 5;
    public float burstSpread = 2f;
    public int damage = 1;

    private float lastShot;
    private float lastScan;
    private ShipModule module;
    private Faction shipFaction;
    
    [SerializeField]private ParticleSystem bullets;

    private class DefenseCell
    {
        public Transform firePoint;
        public ModuleCellScript script;
        public LineRenderer line;
        public Projectile target;
        public float lastShotTime;
    }

    private List<DefenseCell> defenseCells = new();
    public List<Ship> targets = new List<Ship>();

    public void Initialize()
    {
        shipFaction = module.owner.GetComponent<Ship>().faction;
    }
    
    private void Awake()
    {
        module = GetComponent<ShipModule>();
        cooldown = module.cooldown;
        maxRange = module.data.maxRange;
        scanRange = maxRange * 2;
        damage = module.damage;
        
        // Cache all PointDefense cells
        for (int i = 0; i < module.data.shape.Length; i++)
        {
            if (module.data.shape[i].type != OutfitType.PointDefense) continue;

            var cell = module.builder.cells[i];
            var script = cell.GetComponent<ModuleCellScript>();

            // Create LineRenderer
            GameObject lrObj = new GameObject("PD_Line");
            lrObj.transform.SetParent(cell.transform);
            LineRenderer lr = lrObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0f, 1f, 0f, 0.15f);
            lr.endColor = new Color(0f, 1f, 0f, 0.25f);
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.positionCount = 2;
            lr.enabled = false;

            defenseCells.Add(new DefenseCell
            {
                firePoint = cell.transform,
                script = script,
                line = lr,
                target = null
            });
        }
    }

    private void Update()
    {
        //target update
        if (Time.time - lastScan >= cooldown)
            {AssignTargets();lastScan = Time.time;}
        // Rotate + DrawLine every frame
        foreach (var cell in defenseCells)
        {
            if (cell.target != null)
            {
                Vector2 dir = (cell.target.transform.position - cell.firePoint.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                if (cell.script?.outfitSprite != null)
                    cell.script.outfitSprite.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
                if (shipFaction==Faction.Player)
                {
                    cell.line.enabled = true;
                    cell.line.SetPosition(0, cell.firePoint.position);
                    cell.line.SetPosition(1, cell.target.transform.position);
                }
            }
            else
            {
                cell.line.enabled = false;
            }
            // Shooting from each cell
            if (Time.time - cell.lastShotTime>= cooldown)
                if (Fire(cell)) cell.lastShotTime = Time.time;
        }
    }

    private void AssignTargets()
    {
        // Collect all valid targets
        var availableTargets = new List<Projectile>();

        if (ProjectileManager.Instance != null)
        {
            availableTargets.AddRange(
                ProjectileManager.Instance.activeProjectiles
                .Where(p => p != null
                            && p.ownerShipFaction!=shipFaction
                            && Vector3.Distance(transform.position, p.transform.position) <= scanRange)
            );
        }

        // Add ships as fallback targets
        if (targets != null && targets.Count > 0)
        {
            availableTargets.AddRange(
                targets.Where(s => s != null && Vector3.Distance(transform.position, s.transform.position) <= scanRange)
                        .Select(s => s.GetComponent<Projectile>()) // temporary proxy
            );
        }

        // Sort by priority: missile > shell > ship
        availableTargets = availableTargets
            .OrderByDescending(t => t is MissileProjectile ? 3 : (t is Projectile ? 2 : 1))
            .ThenBy(t => Vector3.Distance(transform.position, t.transform.position))
            .ToList();

        // Assign unique targets to cells
        foreach (var cell in defenseCells)
            cell.target = null;

        int count = Mathf.Min(defenseCells.Count, availableTargets.Count);
        for (int i = 0; i < count; i++)
        {
            defenseCells[i].target = availableTargets[i];
        }
    }

    private bool Fire(DefenseCell cell)
    {
        if (cell.target == null) return false;
        if (Vector3.Distance(cell.firePoint.position, cell.target.transform.position) > maxRange) return false;
        for (int i = 0; i < burstCount; i++)
        {
            Vector3 hitPos = cell.target.transform.position + (Vector3)Random.insideUnitCircle * burstSpread;
            ProjectileManager.Instance.SpawnPointDefenseShot(
                cell.firePoint.position,
                hitPos,
                damage,
                maxRange,
                1,
                module.owner
            );
        }

        ProjectileManager.Instance.SpawnBulletEffect(cell.firePoint.position, cell.target.transform.position,1,maxRange, cell.firePoint);
        return true;
    }
}
