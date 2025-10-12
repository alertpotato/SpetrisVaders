using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public StateMachine GameLoopState;
    public Ship playerShip;
    public ModuleFactory MFactory;
    public ShipFactory SFactory;
    public EnemyManager EManager;
    public ModuleSpawner MSpawner;
    public TypewriterMessageQueue HUDConsole;
    
    //Start of game
    [SerializeField]private GameObject ShipChoisePrefab;
    [SerializeField]private GameObject ShipChoiseUI;
    [SerializeField]private Canvas ShipChoise;
    [SerializeField]private GameObject DockerAnchorPrefab;
    [SerializeField]private GameObject DockerGhostPrefab;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        EManager.HUDConsole=HUDConsole;
        HUDConsole.EnqueueMessage("> SYSTEMS REBOOT COMPLETE — ALL GREEN");
        GameLoopState.ChangeState<GameLoopRoundState>();
        CreateShipChoise();
    }

    private void GameStart()
    {
        EManager.enabled=true;
        MSpawner.enabled=true;
        // TODO: spawn enemies, handle waves
    }

    public void CreateShipChoise()
    {
        HUDConsole.EnqueueMessage("> NEW SOFTWARE PATCH DEPLOYED");
        
        var flagship = SFactory.GetShip(new ArchetypeFlagship().moduleWeights,5,faction:Faction.Player,shipAlignment:0);
        var flagshipUI = Instantiate(ShipChoisePrefab, ShipChoiseUI.transform);
        flagshipUI.GetComponent<ShipChoiceUI>().descriptionText.text = "Flagship design";
        flagship.transform.SetParent(flagshipUI.transform);
        flagship.transform.localPosition = Vector3.zero;
        flagship.transform.localScale = new Vector3(30, 30, 30);
        
        var wasp = SFactory.GetShip(new ArchetypeWasp().moduleWeights,5,faction:Faction.Player,shipAlignment:0);
        var waspUI = Instantiate(ShipChoisePrefab, ShipChoiseUI.transform);
        waspUI.GetComponent<ShipChoiceUI>().descriptionText.text = "Wasp design";
        wasp.transform.SetParent(waspUI.transform);
        wasp.transform.localPosition = Vector3.zero;
        wasp.transform.localScale = new Vector3(30, 30, 30);
        
        var patrol = SFactory.GetShip(new ArchetypePatrol().moduleWeights,5,faction:Faction.Player,shipAlignment:0);
        var patrolUI = Instantiate(ShipChoisePrefab, ShipChoiseUI.transform);
        patrolUI.GetComponent<ShipChoiceUI>().descriptionText.text = "Scout design";
        patrol.transform.SetParent(patrolUI.transform);
        patrol.transform.localPosition = Vector3.zero;
        patrol.transform.localScale = new Vector3(25, 25, 25);
        
        flagshipUI.GetComponent<Button>().onClick.AddListener(() => SelectShip(flagship,1));
        waspUI.GetComponent<Button>().onClick.AddListener(() => SelectShip(patrol,2));
        patrolUI.GetComponent<Button>().onClick.AddListener(() => SelectShip(wasp,3));
    }

    public void SelectShip(GameObject ship,int index)
    {
        ship.transform.SetParent(transform);
        ship.transform.localPosition = new Vector3(0,-20,0);
        ship.transform.localScale = new Vector3(1,1,1);
        
        ship.AddComponent<DockingVisualizer>();
        var docker = ship.GetComponent<DockingVisualizer>();
        ship.AddComponent<PlayerController>().Initialize(EManager);
        var anchorParent = new GameObject();
        anchorParent.transform.SetParent(ship.transform);
        anchorParent.transform.localPosition = Vector3.zero;
        anchorParent.transform.localScale = new Vector3(1,1,1);
        anchorParent.name = "DockingAnchor";
        docker.Initialize(DockerAnchorPrefab,DockerGhostPrefab,MSpawner,anchorParent);
        Destroy(ShipChoiseUI);
        
        playerShip = ship.GetComponent<Ship>();
        playerShip.HUDConsole=HUDConsole;
        EManager.playerShip = playerShip;
        EManager.SFactory = SFactory;
        
        HUDConsole.EnqueueMessage("> BATTLESHIP "+ship.name.ToString().ToUpper()+" OPERATIONAL...");
        GameStart();
    }
}