using System.Collections.Generic;
using UnityEngine;
public struct Candidate
{
    public GameObject module;
    public Vector2Int anchor;

    public Candidate(GameObject module, Vector2Int anchor)
    {
        this.module = module;
        this.anchor = anchor;
    }
}

public class DockingCandidatesManager
{
    private LinkedList<Candidate> list = new LinkedList<Candidate>();
    private Dictionary<GameObject, LinkedListNode<Candidate>> lookup = new Dictionary<GameObject, LinkedListNode<Candidate>>();
    
    public void AddOrUpdate(GameObject module, Vector2Int anchor)
    {
        if (module == null) return;

        if (lookup.TryGetValue(module, out var node))
        {
            node.Value = new Candidate(module, anchor);
        }
        else
        {
            var newNode = list.AddLast(new Candidate(module, anchor));
            lookup[module] = newNode;
        }
    }
    
    public bool Remove(GameObject module)
    {
        if (module == null) return false;
        if (lookup.TryGetValue(module, out var node))
        {
            lookup.Remove(module);
            list.Remove(node);
            return true;
        }
        return false;
    }

    public void PurgeMissing(ICollection<GameObject> activeModules)
    {
        var node = list.First;
        while (node != null)
        {
            var next = node.Next;
            if (!activeModules.Contains(node.Value.module) || node.Value.module == null)
            {
                lookup.Remove(node.Value.module);
                list.Remove(node);
            }
            node = next;
        }
    }

    public IEnumerable<Candidate> GetCandidatesInOrder()
    {
        foreach (var n in list) yield return n;
    }

    public int Count => list.Count;
}
