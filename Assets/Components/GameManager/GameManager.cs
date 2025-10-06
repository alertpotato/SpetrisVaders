using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public StateMachine GameLoopState;
    public Ship playerShip;
    public ModuleFactory MFactory;
    public ShipFactory SFactory;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Debug.Log("Game started!");
        //var newObj = MFactory.CreateModule("Canon",Vector3.one,playerShip.transform);
        TestShip();
        GameLoopState.ChangeState<GameLoopRoundState>();
        // TODO: spawn enemies, handle waves
    }
    public void TestShip()
    {
        playerShip.shipAlignment = 0;

        SFactory.RandomAttach(playerShip, out GameObject module1, MFactory.GetCockpitModule(0));
        var randomModule = MFactory.GetModule("Missile",playerShip.transform);
        SFactory.RandomAttach(playerShip, out GameObject module2, randomModule);
        
        playerShip.InitializeShip(Faction.Player);
    }
}