using System.Collections.Generic;
using UnityEngine;
public struct AnchorOption {
    public Vector2Int anchor;
    public Vector2Int adjustment;
    public AnchorOption(Vector2Int anchor, Vector2Int adjustment) {
        this.anchor = anchor;
        this.adjustment = adjustment;
    }
}

public class Candidate {
    public GameObject module;
    public List<AnchorOption> options;
    private int currentIndex;

    public Candidate(GameObject module, List<AnchorOption> options) {
        this.module = module;
        this.options = options;
        this.currentIndex = 0;
    }

    public AnchorOption Primary => options[currentIndex];

    public void UpdateOptions(List<AnchorOption> newOptions) {
        // saving current anchor if still valid
        var current = Primary;
        options = newOptions;
        int idx = options.FindIndex(o => o.anchor == current.anchor && o.adjustment == current.adjustment);
        currentIndex = idx >= 0 ? idx : 0;
    }

    public void CycleAnchor() {
        if (options.Count == 0) return;
        currentIndex = (currentIndex + 1) % options.Count;
    }
}

public class DockingCandidatesManager
{
    private LinkedList<Candidate> list = new();
    private Dictionary<GameObject, LinkedListNode<Candidate>> lookup = new();

    public void AddOrUpdate(GameObject module, List<AnchorOption> options)
    {
        if (module == null || options == null || options.Count == 0) return;

        if (lookup.TryGetValue(module, out var node)) {
            node.Value.UpdateOptions(options);
        } else {
            var newNode = list.AddLast(new Candidate(module, options));
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

    public Candidate GetCandidate(GameObject module) {
        return lookup.TryGetValue(module, out var node) ? node.Value : null;
    }
    public IEnumerable<Candidate> GetCandidatesInOrder() => list;
    public int Count => list.Count;

    public void CycleAnchor(GameObject module)
    {
        if (lookup.TryGetValue(module, out var node))
        {
            node.Value.CycleAnchor();
        }
    }
}
