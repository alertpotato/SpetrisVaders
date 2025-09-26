using UnityEngine;

[RequireComponent(typeof(StateBehaviour))]
[RequireComponent(typeof(GameLoopRoundState))]

public class GameLoopSharedData : MonoBehaviour
{
    [Header("Components")]
    public Camera MainCamera;
    
    public StateMachine GameLoopState;
    
    [Header("Data")]
    
    [Header("Entities")]
    
    [Header("UI")] 
    
    [Header("Variables")]
    public int Day = 1;
    [Header("Prefabs")]
    
    [Header("States")]
    public GameLoopRoundState RoundState;
    
    private void Awake()
    {
        RoundState = transform.GetComponent<GameLoopRoundState>();
        RoundState.Config = this;
    }

    private void Start()
    {
        Day = 0;
    }
}
