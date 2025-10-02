using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SortFunctions
{
    public static List<T> ShuffleList<T>(List<T> list)
    {
        var copy = list.ToList();
        int n = copy.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = Random.Range(i, n);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }
}
