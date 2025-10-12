using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Ship))]
[RequireComponent(typeof(InertialBody))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private EnemyManager enemies;
    [SerializeField] private Ship Ship;
    [SerializeField] private Controls controls;
    [SerializeField] private DockingVisualizer Docker;
    private InertialBody body;

    private Vector2 moveInput;
    private bool canonFire=false;
    private bool missileFire=false;

    private void Awake()
    {
        Ship = GetComponent<Ship>();
        body = GetComponent<InertialBody>();
        Docker = GetComponent<DockingVisualizer>();

        controls = new Controls();
        
        controls.ShipControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.ShipControls.Move.canceled  += ctx => moveInput = Vector2.zero;

        controls.ShipControls.CanonShot.performed += ctx => canonFire = true;
        controls.ShipControls.CanonShot.canceled  += ctx => canonFire = false;
        controls.ShipControls.MissileShot.performed += ctx => missileFire = true;
        controls.ShipControls.MissileShot.canceled  += ctx => missileFire = false;
        
        controls.ShipControls.AttachModule.performed += ctx => AttachModule();
        controls.ShipControls.RotateModule.performed += ctx => RotateModule();
        controls.ShipControls.CycleModuleAnchor.performed += ctx => CycleAnchors();
    }
    public void Initialize(EnemyManager enemyManager)
    {
        enemies = enemyManager;
    }

    private void OnEnable()  => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        HandleMovement();
    }
    private void Update()
    {
        HandleShooting();
    }
    
    private void HandleMovement()
    {
        bool forceApplied = false;
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector2 force = moveInput.normalized * Ship.thrust;
            body.ApplyForce(force, Time.fixedDeltaTime);
            forceApplied = true;
        }
        body.Tick(Time.fixedDeltaTime,isForceApplied:forceApplied);
    }

    private void HandleShooting()
    {
        if (canonFire) Ship.FireCanons();
        if (missileFire) FireMissiles();
    }

    private void FireMissiles()
    {
        Ship.FireMissle(enemies.enemies.Select(x=>x.ship).ToList());
    }

    private void AttachModule()
    {
        if (Docker.candidates.Count == 0) return;
        var candidate = Docker.candidates.GetCandidatesInOrder().First();
        Docker.freeModules.ForgetModule(candidate.module);
        candidate.module.GetComponent<ShipModule>().UpdateRotation(Docker.currentRotation); // get rotation from ghost - apply to ShipModule and ModuleBuilder
        Ship.AttachModule(candidate);
    }
    private void RotateModule()
    {
        if (Docker.candidates.Count == 0) return;
        Docker.RotateModule();
    }

    private void CycleAnchors()
    {
        if (Docker.candidates.Count == 0) return;
        var candidate = Docker.candidates.GetCandidatesInOrder().First();
        candidate.CycleAnchor();
    }
}
