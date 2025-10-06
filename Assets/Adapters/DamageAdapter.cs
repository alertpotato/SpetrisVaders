using UnityEngine;
using UnityEngine.Events;

public class DamageAdapter : MonoBehaviour
{
    public UnityEvent<int> TakeDamage;
    public GameObject owner;
}