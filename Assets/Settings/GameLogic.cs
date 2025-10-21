using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameLogic", menuName = "Settings/Game Logic")]
public class GameLogic : ScriptableObject
{
    public static GameLogic Instance { get; private set; }
    
    [Header("Layers")]
    public string playerLayer;
    public string enemyLayer;
    public string projectileLayer;
    public string environmentLayer;
    public string particleLayer;
    
    private void OnEnable()
    {
        Instance = this;
    }
}