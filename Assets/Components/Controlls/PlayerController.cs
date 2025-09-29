using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Ship))]
public class PlayerController : MonoBehaviour
{
    [FormerlySerializedAs("ship")]
    [Header("Components")]
    [SerializeField]private Ship Ship;
    [SerializeField]private Controls controls;
    [SerializeField]private DockingVisualizer Docker;
    private Vector2 moveInput;
    private bool shooting;
    private bool attaching;

    private void Awake()
    {
        Ship = GetComponent<Ship>();
        controls = new Controls();
        
        controls.ShipControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.ShipControls.Move.canceled  += ctx => moveInput = Vector2.zero;

        controls.ShipControls.CanonShot.performed += ctx => shooting = true;
        controls.ShipControls.CanonShot.canceled  += ctx => shooting = false;
        
        controls.ShipControls.AttachModule.performed += ctx => AttachModule();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        Vector3 dir = new Vector3(moveInput.x, moveInput.y, 0).normalized;
        transform.position += dir * Ship.Speed * Time.deltaTime;
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
        Ship.AttachModule(candidate);
        Docker.freeModules.ForgetModule(candidate.module);
    }
}