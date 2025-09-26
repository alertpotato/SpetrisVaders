using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

[RequireComponent(typeof(GameLoopSharedData))]
public class GameLoopRoundState : StateBehaviour
{
    public GameLoopSharedData Config;
    public int CurrentRound = 1;
    public override void OnEnter()
    {
        CurrentRound = 1;
    }
    public void StartRound()
    {
        Debug.Log($"Entered round {CurrentRound}!");
    }
    public void RoundEnd()
    {
        Debug.Log($"Round {CurrentRound} ended!");
        // Round ending logic
        CurrentRound += 1;
    }
    
    public override void OnExit()
    {
    }
}
