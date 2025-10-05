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
        var newObj = MFactory.GetModule("Missile",playerShip.transform);
        var anchorlist = new List<AnchorOption>();
        var anchor = new AnchorOption(new Vector2Int(0, 0), new Vector2Int(0, 0));
        anchorlist.Add(anchor);
        var can = new Candidate(newObj, anchorlist);
        playerShip.AttachModule(can);
    }
}