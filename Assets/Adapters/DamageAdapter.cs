using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ShipModule))]
public class DamageAdapter : MonoBehaviour
{
    public UnityEvent<int> TakeDamage;
    public GameObject owner;
}