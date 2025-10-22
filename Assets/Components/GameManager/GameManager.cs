using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Components")]
    public static GameManager Instance;
    public StateMachine GameLoopState;
    public GameLoopSharedData LoopSharedData;
    public ModuleFactory MFactory;
    public ShipFactory SFactory;
    public EnemyManager EManager;
    public ModuleSpawner MSpawner;
    public AsteroidSpawner ASpawner;
    public TypewriterMessageQueue HUDConsole;
    public GameObject PlayerControls;
    [Header("Game Graphic Components")] 
    public StarfieldController ScreenStars;
    
    [Header("Player staff")]
    public Ship playerShip;
    [SerializeField]private GameObject DockerAnchorPrefab;
    [SerializeField]private GameObject DockerGhostPrefab;
    [Header("Start event staff")]
    [SerializeField]private GameObject ShipChoisePrefab;
    [SerializeField]private GameObject ShipChoiseUI;
    [SerializeField]private Canvas ShipChoise;
    
    [Header("Settings")]
    public GameGraphics gameGraphics;
    public GameLogic gameLogic;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
    }

    private void Start()
    {
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Confined;
        EManager.HUDConsole=HUDConsole;
        HUDConsole.EnqueueMessage("> SYSTEMS REBOOT COMPLETE — ALL GREEN");
        
        LoopSharedData.HUDConsole = HUDConsole;
        LoopSharedData.EManager = EManager;
        CreateShipChoise();
    }

    private void GameStart()
    {
        EManager.enabled=true;
        MSpawner.enabled=true;
        ASpawner.enabled=true;
        GameLoopState.enabled = true;
        GameLoopState.ChangeState<GameLoopEnemyWaveState>();
        LoopSharedData.playerShip = playerShip;
        //Graphic
        ScreenStars.Initialize(playerShip.GetComponent<InertialBody>());
        // TODO: spawn enemies, handle waves
    }

    public void CreateShipChoise()
    {
        var flagship = SFactory.GetShip(new ArchetypeFlagship().moduleWeights,5,faction:Faction.Player,shipAlignment:0,directionChances:new ArchetypeFlagship().ShipBuildPriority);
        var flagshipUI = Instantiate(ShipChoisePrefab, ShipChoiseUI.transform);
        flagshipUI.GetComponent<ShipChoiceUI>().descriptionText.text = "Flagship";
        flagship.transform.SetParent(flagshipUI.transform);
        flagship.transform.localPosition = Vector3.zero;
        flagship.transform.localScale = new Vector3(30, 30, 30);
        
        var wasp = SFactory.GetShip(new ArchetypeWasp().moduleWeights,5,faction:Faction.Player,shipAlignment:0,directionChances:new ArchetypeWasp().ShipBuildPriority);
        var waspUI = Instantiate(ShipChoisePrefab, ShipChoiseUI.transform);
        waspUI.GetComponent<ShipChoiceUI>().descriptionText.text = "Wasp";
        wasp.transform.SetParent(waspUI.transform);
        wasp.transform.localPosition = Vector3.zero;
        wasp.transform.localScale = new Vector3(30, 30, 30);
        
        var patrol = SFactory.GetShip(new ArchetypePatrol().moduleWeights,5,faction:Faction.Player,shipAlignment:0,directionChances:new ArchetypePatrol().ShipBuildPriority);
        var patrolUI = Instantiate(ShipChoisePrefab, ShipChoiseUI.transform);
        patrolUI.GetComponent<ShipChoiceUI>().descriptionText.text = "Scout";
        patrol.transform.SetParent(patrolUI.transform);
        patrol.transform.localPosition = Vector3.zero;
        patrol.transform.localScale = new Vector3(25, 25, 25);
        
        flagshipUI.GetComponent<Button>().onClick.AddListener(() => SelectShip(flagship,1));
        waspUI.GetComponent<Button>().onClick.AddListener(() => SelectShip(wasp,2));
        patrolUI.GetComponent<Button>().onClick.AddListener(() => SelectShip(patrol,3));
    }

    public void SelectShip(GameObject ship,int index)
    {
        ship.transform.SetParent(transform);
        ship.name = "PlayerShip";
        ship.transform.localPosition = new Vector3(0,-20,0);
        ship.transform.localScale = new Vector3(1,1,1);

        var iBody = ship.GetComponent<InertialBody>();
        ship.AddComponent<DockingVisualizer>();
        var docker = ship.GetComponent<DockingVisualizer>();
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
        
        PlayerControls.GetComponent<PlayerController>().Initialize(playerShip,iBody,docker,EManager, docker);
        
        HUDConsole.EnqueueMessage("> BATTLESHIP "+ship.name.ToString().ToUpper()+" OPERATIONAL...");
        GameStart();
    }
}