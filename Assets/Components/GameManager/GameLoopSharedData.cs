using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(StateBehaviour))]
[RequireComponent(typeof(GameLoopEnemyWaveState))]
[RequireComponent(typeof(GameLoopRewardState))]

public class GameLoopSharedData : MonoBehaviour
{
    [Header("Components")]
    public Camera MainCamera;
    public StateMachine GameLoopState;
    public Ship playerShip;
    [Header("References")]
    public EnemyManager EManager;
    
    [Header("Data")]
    
    [Header("Entities")]
    
    [Header("UI")]
    public TypewriterMessageQueue HUDConsole;
    [Header("Variables")]
    public int CurrentWave = 0;
    [Header("Prefabs")]
    
    [Header("States")]
    public GameLoopEnemyWaveState enemyWaveState;
    public GameLoopRewardState rewardState;
    
    private void Awake()
    {
        MainCamera=Camera.main;
        enemyWaveState = transform.GetComponent<GameLoopEnemyWaveState>();
        enemyWaveState.Config = this;
        rewardState = transform.GetComponent<GameLoopRewardState>();
        rewardState.Config = this;
    }

    private void Start()
    {

    }
}
