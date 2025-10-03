using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Ship))]
[RequireComponent(typeof(InertialBody))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Ship Ship;
    [SerializeField] private Controls controls;
    [SerializeField] private DockingVisualizer Docker;
    private InertialBody body;

    private Vector2 moveInput;
    private bool shooting;

    private void Awake()
    {
        Ship = GetComponent<Ship>();
        body = GetComponent<InertialBody>();

        controls = new Controls();
        
        controls.ShipControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.ShipControls.Move.canceled  += ctx => moveInput = Vector2.zero;

        controls.ShipControls.CanonShot.performed += ctx => shooting = true;
        controls.ShipControls.CanonShot.canceled  += ctx => shooting = false;
        
        controls.ShipControls.AttachModule.performed += ctx => AttachModule();
        controls.ShipControls.RotateModule.performed += ctx => RotateModule();
        controls.ShipControls.CycleModuleAnchor.performed += ctx => CycleAnchors();
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
        body.Tick(Time.fixedDeltaTime,forceApplied);
    }

    private void HandleShooting()
    {
        if (shooting)
        {
            foreach (var module in Ship.modules)
            {
                if (module.data.type == ModuleType.Canon)
                {
                    module.FireCanon(Vector3.up);
                }
            }
        }
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
