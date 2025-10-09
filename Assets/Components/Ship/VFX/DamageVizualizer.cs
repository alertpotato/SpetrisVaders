using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ShipModule))]
[RequireComponent(typeof(ModuleBuilder))]
public class DamageVizualizer : MonoBehaviour
{
    private ShipModule module;
    private ModuleBuilder moduleBuilder;
    [SerializeField]private int cellsNumber;
    [SerializeField]private float moduleHP;
    
    [SerializeField]private List<GameObject> cells;
    [SerializeField]private List<GameObject> damagedCells;
    public float particleCooldown = 5f;
    private float lastEffect = 0;
    void Awake()
    {
        module = GetComponent<ShipModule>();
        moduleBuilder = GetComponent<ModuleBuilder>();
    }
    void Start()
    {
        cellsNumber = module.data.shape.Length;
        moduleHP = module.data.baseHealth;
        cells = new List<GameObject>(moduleBuilder.cells);
        cells = SortFunctions.ShuffleList(cells);
        damagedCells = new List<GameObject>();
    }

    void Update()
    {
        int targetCount = Mathf.Clamp(
            (int)((moduleHP - module.currentHP) / (moduleHP / cellsNumber)),
            0, cellsNumber
        );
        
        while (damagedCells.Count < targetCount)
        {
            var newCell = cells[damagedCells.Count];
            damagedCells.Add(newCell);
            var cellScript = newCell.GetComponent<ModuleCellScript>();
            if (cellScript!=null) cellScript.VizualizeDamage(true);
        }

        while (damagedCells.Count > targetCount)
        {
            var removedCell = damagedCells[^1]; // ^1 = последний элемент
            var cellScript = removedCell.GetComponent<ModuleCellScript>();
            if (cellScript!=null) cellScript.VizualizeDamage(false);
            damagedCells.RemoveAt(damagedCells.Count - 1);
        }
        
        if (Time.time > lastEffect)
        {
            if (targetCount > 0)
            {
                var randomCell = cells[Random.Range(0, cells.Count)];
                var damage = Instantiate(randomCell.GetComponent<ModuleCellScript>().damagedParticles, randomCell.transform);
            }
            lastEffect = Time.time + particleCooldown + particleCooldown*Random.value;
        }

        if (module.currentHP == moduleHP) cells = SortFunctions.ShuffleList(moduleBuilder.cells);
    }
}

