using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public List<Encounter> encounters = new List<Encounter>();

    public Encounter GetEncounter(int round, EncounterType type)
    {
        var possibleEncounters = encounters.Where(x => x.type == type && x.encounterRank >= round).ToList();
        if (possibleEncounters.Count() == 0) {Debug.LogWarning("No suitable encounters found!");
            return null;
        }
        return possibleEncounters[Random.Range(0, possibleEncounters.Count())];
    }
}