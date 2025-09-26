using UnityEngine;

[RequireComponent(typeof(Ship))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]private Ship ship;
    [SerializeField]private Controls controls;

    private Vector2 moveInput;
    private bool shooting;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        controls = new Controls();

        // Подписки на действия
        controls.ShipControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.ShipControls.Move.canceled  += ctx => moveInput = Vector2.zero;

        controls.ShipControls.CanonShot.performed += ctx => shooting = true;
        controls.ShipControls.CanonShot.canceled  += ctx => shooting = false;
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
        transform.position += dir * ship.Speed * Time.deltaTime;
    }

    private void HandleShooting()
    {
        if (shooting)
        {
            foreach (var module in ship.modules)
            {
                if (module.data.type == ModuleType.Canon)
                {
                    module.FireCanon(Vector3.up);
                }
            }
        }
    }
}