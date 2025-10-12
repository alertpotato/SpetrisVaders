using System.Collections.Generic;using System.Linq;
using UnityEngine;
public enum EncounterType {Wave, Boss}
[CreateAssetMenu(fileName = "Encounter", menuName = "Game/Encounter Data")]
public class Encounter : ScriptableObject
{
    public string name;
    public EncounterType type;
    public float difficultyMultiplier;
    public ShipArchetypeType fleetLeader;
    public float fleetLeaderDifficultyMultiplier;
    public List<ShipArchetypeType> guaranteedShips;
    public int numberOfEnemies;
    public float timeBetweenSpawns;
    public int encounterRank;
}