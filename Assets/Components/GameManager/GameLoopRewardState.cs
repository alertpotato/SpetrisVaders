using System;
using UnityEngine;

[RequireComponent(typeof(GameLoopSharedData))]
public class GameLoopRewardState : StateBehaviour
{
    public GameLoopSharedData Config;

    public override void OnEnter()
    {
        Config.GameLoopState.ChangeState<GameLoopEnemyWaveState>();
    }
    public override void OnExit()
    {
        
    }
}