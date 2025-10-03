using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightFunctions
{
    public static int GetRandomWeightedIndex(int[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;

        int weightSum = 0;
        int i;
        for (i = 0; i < weights.Length; i++)
        {
            if (weights[i] >= 0) weightSum += weights[i];
        }

        float r = Random.value;
        float s = 0f;

        for (i = 0; i < weights.Length; i++)
        {
            if (weights[i] <= 0f) continue;

            s += (float)weights[i] / weightSum;
            if (s >= r) return i;
        }

        return -1;
    }
    public static ModuleType GetRandomWeightedModule(Dictionary<ModuleType, int> moduleWeights)
    {
        var types = new List<ModuleType>();
        var weights = new List<int>();

        foreach (var kvp in moduleWeights)
        {
            if (kvp.Value > 0)
            {
                types.Add(kvp.Key);
                weights.Add(kvp.Value);
            }
        }
        //TODO is that ok?
        if (weights.Count == 0) return ModuleType.Empty;
        
        int weightSum = 0;
        foreach (int w in weights) weightSum += w;

        float r = UnityEngine.Random.value;
        float s = 0f;

        for (int i = 0; i < weights.Count; i++)
        {
            s += (float)weights[i] / weightSum;
            if (s >= r)
            {
                return types[i];
            }
        }

        return types[types.Count - 1];
    }
}
