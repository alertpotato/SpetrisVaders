using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private EnemyManager enemies;
    [SerializeField] private Ship Ship;
    [SerializeField] private Controls controls;
    [SerializeField] private DockingVisualizer Docker;
    private InertialBody Body;
    Camera mainCamera;
    private Vector2 moveInput;
    private bool canonFire=false;
    private bool missileFire=false;
    [Header("Variables")]
    private bool ShipControlled = false;
    Vector3 mouseWorldPosition = Vector3.zero;
    [Header("Aim Graphic")]
    [SerializeField] private CursorController cursor;
    [SerializeField] private EnemyScan Scan;

    private void Awake()
    {
        mainCamera = Camera.main;
        controls = new Controls();
        
        controls.ShipControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.ShipControls.Move.canceled  += ctx => moveInput = Vector2.zero;

        controls.ShipControls.CanonShot.performed += ctx => canonFire = true;
        controls.ShipControls.CanonShot.canceled  += ctx => canonFire = false;
        controls.ShipControls.MissileShot.performed += ctx => missileFire = true;
        controls.ShipControls.MissileShot.canceled  += ctx => missileFire = false;
        controls.ShipControls.Fire.performed += ctx => Fire();
        
        controls.ShipControls.AttachModule.performed += ctx => AttachModule();
        controls.ShipControls.RotateModule.performed += ctx => RotateModule();
        controls.ShipControls.CycleModuleAnchor.performed += ctx => CycleAnchors();

        controls.ShipControls.RestartGame.performed += ctx =>RestartGame();
    }
    public void Initialize(Ship Pship, InertialBody Ibody, DockingVisualizer Pdocker, EnemyManager enemyManager)
    {
        Ship = Pship;
        Body = Ibody;
        Docker = Pdocker;
        enemies = enemyManager;
        ShipControlled = true;
        cursor.ChangeMode(CursorMode.Circle);
        Scan.Initialize(mainCamera);
    }

    private void OnEnable()  => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        if (ShipControlled)
        {
            HandleMovement();
            var scan = EnemyScan();
        }
    }
    private void Update()
    {
        if (ShipControlled) HandleShooting();
        //CURSOR
        Vector3 mouseScreen = Input.mousePosition;
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorldPosition.z = 0f;
        cursor.AdjastPosition(mouseWorldPosition);
    }

    private Ship EnemyScan()
    {
        if (enemies.enemies.Count == 0) return null;
        float distanceToCursor = 999;
        Ship scannedShip = null;
        EnemyEntry entry = new EnemyEntry();
        foreach (var enemy in enemies.enemies)
        {
            Ship newShip = enemy.ship;
            var newDist = Vector3.Distance(newShip.transform.position, mouseWorldPosition);
            if (newDist>Vector2.Distance(newShip.dimensionsMin,newShip.dimensionsMax)) continue;
            if (newDist < distanceToCursor)
            { distanceToCursor = newDist; scannedShip = newShip; entry = enemy;}
        }
        if (scannedShip != null)
            Scan.ActivateScan(scannedShip.transform.position, scannedShip.dimensionsMin, scannedShip.dimensionsMax,entry.archetype.type.ToString());
        else Scan.DeactivateScan();
        return scannedShip;
    }

    private void HandleMovement()
    {
        bool forceApplied = false;
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector2 force = moveInput.normalized * Ship.thrust;
            Body.ApplyForce(force, Time.fixedDeltaTime);
            forceApplied = true;
        }
        Body.Tick(Time.fixedDeltaTime,isForceApplied:forceApplied);
    }
    private void Fire()
    {
        if (!ShipControlled) return;
        
        if (Ship.FireAt(mouseWorldPosition)) cursor.Pulse();
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
    private void RestartGame()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex);
    }
}
