using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public StateMachine GameLoopState;
    public Ship playerShip;
    public ModuleFactory MFactory;

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
        var newObj = MFactory.CreateModule("Canon",Vector3.one,playerShip.transform);
        playerShip.AttachModule(newObj.GetComponent<ShipModule>(),Vector2Int.zero);
    }
}