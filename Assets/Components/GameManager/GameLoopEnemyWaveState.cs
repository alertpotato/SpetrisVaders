using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


[RequireComponent(typeof(GameLoopSharedData))]
public class GameLoopEnemyWaveState : StateBehaviour
{
    public GameLoopSharedData Config;
    public bool encounterDeployed=false;
    public EncounterManager ENCManager;
    public override void OnEnter()
    {
        encounterDeployed = false;
        Config.CurrentWave++;
        EncounterType newEncounterType = EncounterType.Wave;
        if (Config.CurrentWave % 2 == 0) newEncounterType = EncounterType.Boss;
        var newEncounter = ENCManager.GetEncounter(Config.CurrentWave,newEncounterType);
        Config.HUDConsole.EnqueueMessage("> load /levels/level"+Config.CurrentWave+"/"+newEncounter.name);
        float waveDifficulty = 1+(Config.CurrentWave-1)*0.5f;
        StartCoroutine(RunEncounter(newEncounter,waveDifficulty) );
    }
    public IEnumerator RunEncounter(Encounter encounter, float waveDifficulty)
    {
        int totalEnemies = encounter.numberOfEnemies + (int)waveDifficulty;
        float difficulty = waveDifficulty * encounter.difficultyMultiplier;
        //Leader first
        float bossDifficulty = difficulty * encounter.fleetLeaderDifficultyMultiplier;
        Config.EManager.SpawnShip(encounter.fleetLeader, bossDifficulty);
        totalEnemies--;
        yield return new WaitForSeconds(encounter.timeBetweenSpawns);

        foreach (var shipType in encounter.guaranteedShips)
        {
            Config.EManager.SpawnShip(shipType, difficulty);
            yield return new WaitForSeconds(encounter.timeBetweenSpawns);
            totalEnemies--;
            if (totalEnemies<=0)break;
        }
        
        if (totalEnemies > 0)
        {
            for (int i = 0; i < totalEnemies; i++)
            {
                ShipArchetype baseArchetype = Config.EManager.archetypes[Random.Range(0, Config.EManager.archetypes.Count)];
                Config.EManager.SpawnShip(baseArchetype.type, difficulty);
                yield return new WaitForSeconds(encounter.timeBetweenSpawns);
            }
        }
        encounterDeployed=true;
    }
    
    public override void OnUpdate()
    {
        if (encounterDeployed == true && Config.EManager.enemies.Count == 0)
        {
            Config.GameLoopState.ChangeState<GameLoopRewardState>();
        }
    }
    
    public override void OnExit()
    {
    }
}
