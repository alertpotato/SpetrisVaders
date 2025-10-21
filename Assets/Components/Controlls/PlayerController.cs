using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum FiringMode {None, Canons, Missiles, PD}
public enum ControlMode { Combat, Docking }

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyManager enemies;
    [SerializeField] private Ship Ship;
    [SerializeField] private Controls controls;
    [SerializeField] private DockingVisualizer Docker;
    [SerializeField] private CursorTooltip cursorTooltip;
    [SerializeField] private DockingVisualizer dockingVisualizer;
    private InertialBody Body;
    Camera mainCamera;
    
    [Header("Variables")]
    private Vector2 moveInput;
    private bool fire;
    Vector3 mouseWorldPosition = Vector3.zero;
    
    [Header("States")]
    public ControlMode controlMode = ControlMode.Combat;
    public FiringMode currentFiringMode = FiringMode.None;
    public FiringMode lastFiringMode = FiringMode.None;
    private bool ShipControlled = false;
    
    [Header("Aim Graphic")]
    [SerializeField] private CursorController cursor;
    [SerializeField] private EnemyScan Scan;
    private GameObject dockingCandidate = null;
    //Firing mode logic
    private bool HasCanons => Ship.modules.Any(m => m.data.type == ModuleType.Canon && m.isFunctioning);
    private bool HasMissiles => Ship.modules.Any(m => m.data.type == ModuleType.Missile && m.isFunctioning);
    private bool HasPD => Ship.modules.Any(m => m.data.type == ModuleType.PointDefense && m.isFunctioning);
    private void Awake()
    {
        mainCamera = Camera.main;
        controls = new Controls();
        cursorTooltip = GetComponent<CursorTooltip>();
        
        controls.ShipControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.ShipControls.Move.canceled  += ctx => moveInput = Vector2.zero;
        
        controls.ShipControls.PrimaryAction.performed += ctx => fire = true;
        controls.ShipControls.PrimaryAction.canceled += ctx => fire = false;
        controls.ShipControls.PrimaryAction.performed += ctx => AttachModule();
        
        controls.ShipControls.Scroll.performed += ctx =>
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                int dir = scroll > 0 ? 1 : -1;
                if (controlMode == ControlMode.Docking)
                    RotateModule(dir);
                else
                    NextFiringMode(dir);
            }
        };
        controls.ShipControls.QuickNextFireMode.performed += ctx => NextFiringMode(1);
        controls.ShipControls.FireModeCanons.performed += ctx => SwitchFiringMode(FiringMode.Canons);
        controls.ShipControls.FireModeMissiles.performed += ctx => SwitchFiringMode(FiringMode.Missiles);
        controls.ShipControls.FireModePD.performed += ctx => SwitchFiringMode(FiringMode.PD);

        controls.ShipControls.SecondaryAction.performed += ctx => StartDocking();
        controls.ShipControls.AttachModule.performed += ctx => AttachModule();
        controls.ShipControls.RotateModule.performed += ctx => RotateModule(1);

        controls.ShipControls.RestartGame.performed += ctx =>RestartGame();
    }
    public void Initialize(Ship Pship, InertialBody Ibody, DockingVisualizer Pdocker, EnemyManager enemyManager, DockingVisualizer newDockingVisualizer)
    {
        Ship = Pship;
        Body = Ibody;
        Docker = Pdocker;
        enemies = enemyManager;
        dockingVisualizer = newDockingVisualizer;
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
            EnemyScan();
        }
    }
    private void Update()
    {
        //CURSOR
        Vector3 mouseScreen = Input.mousePosition;
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorldPosition.z = 0f;
        cursor.AdjastPosition(mouseWorldPosition,Ship== null ? Vector2.zero : mouseWorldPosition - Ship.transform.position);
        //Cursor Tooltip
        cursorTooltip.UpdatePosition(mouseWorldPosition);
        //Fire and movement
        if (!ShipControlled) return;
        Ship.UpdateModulesControl(mouseWorldPosition);
        HandleShooting();
        HandleDocking();
        
    }

    private Ship EnemyScan()
    {
        if (enemies.enemies.Count == 0) {Scan.DeactivateScan();return null;}
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
        {
            
            Scan.ActivateScan(scannedShip,scannedShip.transform.position, scannedShip.dimensionsMin, scannedShip.dimensionsMax,
                entry.archetype.type.ToString());
        }
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
        switch (currentFiringMode)
        {
            case FiringMode.Canons:
                if (Ship.FireAt(mouseWorldPosition)) cursor.Pulse();
                break;
            case FiringMode.Missiles:
                if (Ship.FireMissle(mouseWorldPosition)) cursor.Pulse();
                break;
            case FiringMode.PD:
                if (Ship.FireAtPD(mouseWorldPosition)) cursor.Pulse();
                break;
        }
    }
    private void HandleShooting()
    {
        if (fire) Fire();
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
        lastFiringMode = currentFiringMode;
        if (HasCanons && mode==FiringMode.Canons) currentFiringMode = FiringMode.Canons;
        else if (HasMissiles && mode == FiringMode.Missiles) currentFiringMode = FiringMode.Missiles;
        else if (HasPD && mode == FiringMode.PD) currentFiringMode = FiringMode.PD;
        else if (mode == FiringMode.None) currentFiringMode = FiringMode.None;
        HandleFiringMode();
    }
    private void HandleFiringMode()
    {
        switch (currentFiringMode)
        {
            case FiringMode.Canons:
                cursor.ChangeMode(CursorMode.Circle);
                Ship.ControlModulesByType(ModuleType.Canon);
                cursorTooltip.UpdateTooltip("main gun x"+Ship.modules.Where(x=>x.data.type==ModuleType.Canon).Count());
                break;
            case FiringMode.Missiles:
                cursor.ChangeMode(CursorMode.Triangle);
                Ship.ControlModulesByType(ModuleType.Missile);
                cursorTooltip.UpdateTooltip("missile x"+Ship.modules.Where(x=>x.data.type==ModuleType.Missile).Count());
                break;
            case FiringMode.PD:
                cursor.ChangeMode(CursorMode.Square);
                Ship.ControlModulesByType(ModuleType.PointDefense);
                cursorTooltip.UpdateTooltip("autocanon x"+Ship.modules.Where(x=>x.data.type==ModuleType.PointDefense).Count());
                break;
            case FiringMode.None:
                cursor.ChangeMode(CursorMode.System);
                Ship.ControlModulesByType(ModuleType.Empty);
                break;
        }
    }
    private void RestartGame()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex);
    }
    //---------------DOCKING
    private void StartDocking()
    {
        var detected = DetectColliderUnderMouse(LayerMask.GetMask(GameLogic.Instance.environmentLayer));
        var candidateModule = detected ? detected.GetComponent<ShipModule>() : null;

        if (controlMode != ControlMode.Docking && candidateModule != null)
        {
            EnterDockingMode(candidateModule.gameObject);
        }
        else if (controlMode == ControlMode.Docking)
        {
            ExitDockingMode();
        }
    }

    private void HandleDocking()
    {
        var detected = DetectColliderUnderMouse(LayerMask.GetMask(GameLogic.Instance.environmentLayer));
        if (detected!=null && controlMode != ControlMode.Docking)
        {
            cursorTooltip.UpdateTooltip("RMB to start docking",0.5f);
        }

        if (controlMode == ControlMode.Docking)
        {
            dockingVisualizer.UpdateDocking(mouseWorldPosition,dockingCandidate);
            cursorTooltip.UpdateTooltip("Docking mode\n LMB=Attach, RBM=cancel, Wheel=Rotate");
        }
    }
    private void AttachModule()
    {
        if (!ShipControlled || controlMode!=ControlMode.Docking || Docker?.currentShipModule == null) return;
        var module = Docker.currentShipModule;
        var option = Docker.closestOption;

        Docker.freeModules.ForgetModule(module);
        module.GetComponent<ShipModule>().UpdateRotation(Docker.currentRotation);
        Ship.AttachModule(module, option.anchor, option.adjustment);

        Docker.currentShipModule = null;
        ExitDockingMode();
    }
    private void RotateModule(int dir)
    {
        if (Docker.currentShipModule == null) return;
        int delta = (dir >= 0) ? +90 : -90;
        Docker.RotateModule(delta);
    }
    private void EnterDockingMode(GameObject candidate)
    {
        controlMode = ControlMode.Docking;
        dockingCandidate = candidate;
        SwitchFiringMode(FiringMode.None);
    }

    private void ExitDockingMode()
    {
        controlMode = ControlMode.Combat;
        dockingCandidate = null;
        dockingVisualizer.ClearVisuals();
        SwitchFiringMode(lastFiringMode);
    }
    
    // Cursor Tooltip
    private Collider2D DetectColliderUnderMouse(LayerMask detectionMask)
    {
        Vector2 mousePos2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
        
        return Physics2D.OverlapPoint(mousePos2D, detectionMask);
    }
}
