using UnityEngine;

[RequireComponent(typeof(Ship))]
public class EnemyController : MonoBehaviour
{
    private Ship ship;

    private void Awake()
    {
        ship = GetComponent<Ship>();
    }

    private void Update()
    {
        
    }
}