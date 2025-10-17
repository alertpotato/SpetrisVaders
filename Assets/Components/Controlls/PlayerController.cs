using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum FiringMode {None, Canons, Missiles, PD}
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
    private bool fire;
    private bool canonFire=false;
    private bool missileFire=false;
    [Header("Variables")]
    private bool ShipControlled = false;
    Vector3 mouseWorldPosition = Vector3.zero;
    public FiringMode currentFiringMode = FiringMode.None;
    [Header("Aim Graphic")]
    [SerializeField] private CursorController cursor;
    [SerializeField] private EnemyScan Scan;
    //Firing mode logic
    private bool HasCanons => Ship.modules.Any(m => m.data.type == ModuleType.Canon);
    private bool HasMissiles => Ship.modules.Any(m => m.data.type == ModuleType.Missile);
    private bool HasPD => Ship.modules.Any(m => m.data.type == ModuleType.PointDefense);
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
        controls.ShipControls.Fire.performed += ctx => fire = true;
        controls.ShipControls.Fire.canceled += ctx => fire = false;
        
        controls.ShipControls.NextFireMode.performed += ctx =>
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                int dir = scroll > 0 ? 1 : -1;
                NextFiringMode(dir);
            }
        };
        controls.ShipControls.QuickNextFireMode.performed += ctx => NextFiringMode(1);
        controls.ShipControls.FireModeCanons.performed += ctx => SwitchFiringMode(FiringMode.Canons);
        controls.ShipControls.FireModeMissiles.performed += ctx => SwitchFiringMode(FiringMode.Missiles);
        controls.ShipControls.FireModePD.performed += ctx => SwitchFiringMode(FiringMode.PD);
        
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
        Scan.Initialize(mainCamera);
        NextFiringMode(1);
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
        //CURSOR
        Vector3 mouseScreen = Input.mousePosition;
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorldPosition.z = 0f;
        cursor.AdjastPosition(mouseWorldPosition,Ship== null ? Vector2.zero : mouseWorldPosition - Ship.transform.position);
        if (ShipControlled)
        {
            Ship.UpdateModulesControl(mouseWorldPosition);
            HandleShooting();
        }
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
            if (newDist>Vector2.Distance(newShip.dimensionsMin,newShip.dimensionsMax)*1.2f+5) continue;
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
        switch (currentFiringMode)
        {
            case FiringMode.Canons:
                if (Ship.FireAt(mouseWorldPosition)) cursor.Pulse();
                break;
            case FiringMode.Missiles:
                FireMissiles();
                break;
            case FiringMode.PD:
                if (Ship.FireAtPD(mouseWorldPosition)) cursor.Pulse();
                break;
        }
    }
    private void HandleShooting()
    {
        if (fire) Fire();
        if (canonFire) Ship.FireCanons();
        if (missileFire) FireMissiles();
    }
    private void NextFiringMode(int next = 1)
    {
        var available = new List<FiringMode>();
        if (HasCanons) available.Add(FiringMode.Canons);
        if (HasMissiles) available.Add(FiringMode.Missiles);
        if (HasPD) available.Add(FiringMode.PD);

        if (available.Count == 0)
        {
            currentFiringMode = FiringMode.None;
            return;
        }
        
        int currentIndex = available.IndexOf(currentFiringMode);
        if (currentFiringMode == FiringMode.None || currentIndex < 0)
            currentIndex = (next >= 0) ? 0 : available.Count - 1;
        
        int nextIndex = currentIndex + next;
        if (nextIndex < 0) nextIndex += available.Count;
        else if (nextIndex >= available.Count) nextIndex -= available.Count;

        currentFiringMode = available[nextIndex];
        HandleFiringMode();
    }

    private void SwitchFiringMode(FiringMode mode)
    {
        if (HasCanons && mode==FiringMode.Canons) currentFiringMode = FiringMode.Canons;
        else if (HasMissiles && mode == FiringMode.Missiles) currentFiringMode = FiringMode.Missiles;
        else if (HasPD && mode == FiringMode.PD) currentFiringMode = FiringMode.PD;
        HandleFiringMode();
    }
    private void HandleFiringMode()
    {
        switch (currentFiringMode)
        {
            case FiringMode.Canons:
                cursor.ChangeMode(CursorMode.Circle);
                Ship.ControlModulesByType(ModuleType.Canon);
                break;
            case FiringMode.Missiles:
                cursor.ChangeMode(CursorMode.Triangle);
                Ship.ControlModulesByType(ModuleType.Missile);
                break;
            case FiringMode.PD:
                cursor.ChangeMode(CursorMode.Square);
                Ship.ControlModulesByType(ModuleType.PointDefense);
                break;
            case FiringMode.None:
                cursor.ChangeMode(CursorMode.System);
                Ship.ControlModulesByType(ModuleType.Empty);
                break;
        }
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
